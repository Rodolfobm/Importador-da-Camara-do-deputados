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
        public void importaSessao(DateTime data)
        {
            try
            {
                using (AuditoriaEntities db = new AuditoriaEntities())
                {
                    SessoesReunioes cliente = new SessoesReunioes();
                    sessao_camara sessao = new sessao_camara();
                    List<sessao_camara> sessoes = new List<sessao_camara>();
                    XmlNode resposta = cliente.ListarPresencasDia(data.Date.ToString("dd/MM/yyyy", CultureInfo.CurrentCulture), "", "", "");
                    XmlNodeReader reader = new XmlNodeReader(resposta);
                    int qtdSessoaDia = 0;
                    reader.Read(); //header
                    reader.Read(); //dia
                    if (reader.NodeType.ToString() != "None")
                    {
                        sessao.dataSessao = data; //data
                        reader.Read();
                        reader.Read();
                        reader.Read();             
                        qtdSessoaDia = reader.ReadElementContentAsInt(); //qtdeSessoes
                        sessao.legislatura = reader.ReadElementContentAsInt(); //legislatura
                        //Verifica se o dia já foi importado
                        var dataverify = from d in db.sessoes_camara
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
                                sessoes[i] = db.sessoes_camara.Add(sessoes[i]);
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
        private void importaPresenca(System.Xml.XmlNode resultado, AuditoriaEntities db, List<sessao_camara> sessoes)
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
                        var verify = from b in sessoes.Where(b => b.descricao == descricao)
                                     select b;
                        presenca.idSessao = verify.ElementAt(0).idSessao;
                        if (reader.ReadElementContentAsString() == "Presença")
                        {
                            presenca.presenca = 1;
                        }
                        else
                        {
                            presenca.presenca = 0;
                        }
                        db.presencas_deputado.Add(presenca);
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
            DateTime dataInicio;
            dataFinal = DateTime.Now;
            int ano0 = dataFinal.Year - 2015;
            string param = "";

            using (AuditoriaEntities db = new AuditoriaEntities())
            {
                var dataVerify = from b in db.sessoes_camara
                                 select b;
                if (dataVerify.Count() != 0)
                {
                    List<sessao_camara> sessoesImportadas = dataVerify.ToList();
                    sessao_camara sessao = sessoesImportadas.ElementAt(0);
                    for (int i=1; i < sessoesImportadas.Count(); i++)
                    {
                        if(sessoesImportadas.ElementAt(i).idSessao > sessao.idSessao)
                        {
                            sessao = sessoesImportadas.ElementAt(i);
                        }
                    }
                    //ALTERAR - POSSIVELMENTE 2 LINQS
                    dataInicio = sessao.dataSessao;
                    var deleteVerify = from b in db.presencas_deputado
                                       where (b.idSessao == sessao.idSessao)
                                       select b;
                    db.presencas_deputado.RemoveRange(deleteVerify.ToList());
                    db.sessoes_camara.Remove(sessao);
                    db.SaveChangesAsync();
                }
                else
                {
                    if ((ano0 < 4))
                    {
                        param = "01/02/" + 2015;
                        dataInicio = new DateTime(2015, 02, 01);
                    }
                    else
                    {
                        dataInicio = new DateTime((dataFinal.Year - (ano0 % 4)), 02, 01);
                    }
                }
            }
            while (dataInicio.Date != dataFinal.Date)
            {
                importaSessao(dataInicio);
                dataInicio = dataInicio.AddDays(1);
            }
        }
        public void importaDia()
        {      
              DateTime hoje = new DateTime();
              hoje = DateTime.Now;
              hoje = hoje.AddDays(-5);
              importaSessao(hoje);      
        }
    }
}
