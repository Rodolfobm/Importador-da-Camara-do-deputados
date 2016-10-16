using ImportadorCamaraProcessIcon.br.leg.camara.www;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ImportadorCamaraProcessIcon
{
    class Importador
    {
        public void importaSessao(string data)
        {
            try
            {
                using (auditoriaEntities db = new auditoriaEntities())
                {
                    SessoesReunioes cliente = new SessoesReunioes();
                    sessao_camara sessao = new sessao_camara();
                    List<sessao_camara> sessoes = new List<sessao_camara>();
                    XmlNode resposta = cliente.ListarPresencasDia(data, "", "", "");
                    XmlNodeReader reader = new XmlNodeReader(resposta);
                    int qtdSessoaDia = 0;
                    reader.Read(); //header
                    reader.Read(); //dia
                    if (reader.NodeType.ToString() != "None")
                    {
                        sessao.dataSessao = reader.ReadElementContentAsString(); //data             
                        qtdSessoaDia = reader.ReadElementContentAsInt(); //qtdeSessoes
                        sessao.legislatura = reader.ReadElementContentAsInt(); //legislatura
                        //Verifica se o dia já foi importado
                        var dataverify = from d in db.sessao_camara
                                         .Where(d => d.dataSessao == sessao.dataSessao)
                                         select d;

                        if (!(dataverify.Count() > 0))
                        {
                            reader.Read(); //parlamentar
                            while (reader.Name != "sessaoDia")
                            {
                                reader.Read();
                            }
                            for (int i = 0; i < qtdSessoaDia; i++)
                            {
                                reader.Read();
                                sessao.inicio = reader.ReadElementContentAsString();
                                sessao.inicio = sessao.inicio.Substring(sessao.inicio.IndexOf(" ") + 1, (sessao.inicio.Length - sessao.inicio.IndexOf(" ") - 1));
                                string desc = reader.ReadElementContentAsString();
                                sessao.descricao = desc.Substring(0, desc.IndexOf("-") - 1);
                                reader.Read();
                                reader.Read();
                                reader.Read();
                                reader.Read();

                                sessoes.Add(new sessao_camara()
                                {
                                    dataSessao = sessao.dataSessao,
                                    descricao = sessao.descricao,
                                    inicio = sessao.inicio,
                                    legislatura = sessao.legislatura,
                                });
                                sessoes[i] = db.sessao_camara.Add(sessoes[i]);
                                db.SaveChangesAsync();
                            }
                            importaPresenca(resposta, db, sessoes);
                        }
                    }
                    else
                    {
                    }
                }
            }
            catch (DbEntityValidationException e)
            {
                //gerar log
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine(eve.Entry.Entity.GetType().Name + ": \n");
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("-- " + ve.ErrorMessage + "\n");
                    }
                }
            }
            catch (Exception ex)
            {
                //gerar log
                Console.WriteLine(ex.Message + "\n");
                Console.WriteLine(ex.TargetSite + "\n");
                Console.WriteLine(ex.StackTrace);
                Console.Read();
            }
        }
        private void importaPresenca(System.Xml.XmlNode resultado, auditoriaEntities db, List<sessao_camara> sessoes)
        {
            try
            {
                XmlNodeReader reader = new XmlNodeReader(resultado);
                presenca_deputado presenca = new presenca_deputado();
                bool fim = false;

                while (reader.LocalName != "carteiraParlamentar")
                {
                    reader.Read();
                }
                while (fim == false)
                {
                    presenca.legislatura = sessoes[0].legislatura;
                    presenca.carteiraParlamentar = reader.ReadElementContentAsInt();
                    while (reader.LocalName != "justificativa")
                    {
                        reader.Read();
                    }
                    presenca.justificativa = reader.ReadElementContentAsString();
                    presenca.presencaExterna = (sbyte)reader.ReadElementContentAsInt();
                    reader.Read();
                    while (reader.LocalName != "sessoesDia")
                    {
                        reader.Read();
                        reader.Read();
                        reader.Read();
                        reader.Read();
                        string descricao = reader.ReadElementContentAsString();
                        descricao = descricao.Substring(0, descricao.IndexOf("-") - 1);
                        //Xml não possui consistência na ordem e na quantidade de sessões em diferentes deputados
                        var verify = from b in sessoes.Where(b => b.descricao == descricao)
                                     select b;
                        presenca.sessao_camara = verify.ElementAt(0);
                        if (reader.ReadElementContentAsString() == "Presença")
                        {
                            presenca.presenca = 1;
                        }
                        else
                        {
                            presenca.presenca = 0;
                        }
                        db.presenca_deputado.Add(presenca);
                        db.SaveChangesAsync();
                        reader.Read();
                    }
                    reader.Read();
                    reader.Read();
                    reader.Read();

                    if (reader.NodeType.ToString() == "EndElement")
                    {
                        fim = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\n");
                Console.WriteLine(ex.TargetSite + "\n");
                Console.WriteLine(ex.StackTrace);
                Console.ReadLine();
            }
        }
        public void inicializar()
        {
            DateTime dataFinal = new DateTime();
            DateTime dataInicio = new DateTime();
            dataFinal = DateTime.Now;
            int ano0 = dataFinal.Year - 2015;
            string param = "";

            if ((ano0 < 4))
            {
                param = "01/02/" + 2015;
                dataInicio = DateTime.Parse(param, CultureInfo.CurrentCulture);
            }
            else
            {
                param = "01/02/" + ((int)dataFinal.Year - (ano0 % 4));
                dataInicio = DateTime.Parse(param, CultureInfo.CurrentCulture);
            }

            while (param != dataFinal.Date.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.CurrentCulture).Substring(0, dataFinal.Date.ToString().IndexOf(" ")))
            {
                this.importaSessao(param);
                dataInicio = dataInicio.AddDays(1);
                param = dataInicio.Date.ToString("dd/MM/yyyy", CultureInfo.CurrentCulture).Substring(0, dataInicio.Date.ToString().IndexOf(" "));
            }
        }
        public void controlaMetodo()
        {
            using(auditoriaEntities db = new auditoriaEntities())
            {
                var dataverify = from d in db.sessao_camara
                                 select d;
                if(!(dataverify.Count() > 0))
                {
                    DateTime hoje = new DateTime();
                    hoje = DateTime.Now;
                    hoje = hoje.AddDays(-5);
                    importaSessao(hoje.Date.ToString("dd/MM/yyyy", CultureInfo.CurrentCulture).Substring(0, hoje.Date.ToString().IndexOf(" ")));
                }
                else{
                    inicializar();
                }          
            }
               
        }

    }
}
