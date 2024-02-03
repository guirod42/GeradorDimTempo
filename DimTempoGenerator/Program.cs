using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace DimTempoGenerator
{
    public class Program
    {
        static void Main()
        {
            int primeiroAno = 2000;
            int quantidadeAnos = 1;
            string caminho = @"C:\Users\guisi\OneDrive\Documentos\";
            string nomeTabela = "DimTempo";

            Console.WriteLine("Antes de executar");
            Executar(nomeTabela, caminho, primeiroAno, quantidadeAnos);
            Console.WriteLine("Depois de executar");
        }

        /// <summary>
        /// Função principal que executa as demais operações
        /// </summary>
        /// <param name="nomeTabela">Nome da tabela que será criada.</param>
        /// <param name="caminho">Endereço onde o arquivo será salvo.</param>
        /// <param name="primeiroAno">Primeiro ano de registro.</param>
        /// <param name="quantidadeAnos">Quantidade de anos para registro.</param>
        /// <returns>Cria dois arquivos no endereço fornecido que contém: 1 scrip de criação da tabela e inserção dos registros; 1 script (update) com os principais feriados nacionais consultados em API pública.</returns>
        public static void Executar(string nomeTabela, string caminho, int primeiroAno, int quantidadeAnos)
        {
            Console.WriteLine("Antes de executar");
            CriarArquivos(nomeTabela, caminho);
            ScriptTabela(nomeTabela, caminho, primeiroAno, quantidadeAnos);
            BuscarFeriados(nomeTabela, caminho, primeiroAno, quantidadeAnos);
            Console.WriteLine("Depois de executar");
        }

        /// <summary>
        /// Verifica a existencia dos arquivos, faz a criação se ainda não existir
        /// </summary>
        /// <param name="nomeTabela">Nome da tabela que será criada.</param>
        /// <param name="caminho">Endereço onde o arquivo será salvo.</param>
        /// <returns>Cria dois arquivos no endereço fornecido em branco para gravação dos dados.</returns>
        private static void CriarArquivos(string nomeTabela, string caminho)
        {
            string arquivoScript = @caminho + nomeTabela + "Script.sql";
            string feriadosScript = @caminho + nomeTabela + "Feriados.sql";

            try
            {
                if (File.Exists(arquivoScript))
                {
                    File.Delete(arquivoScript);
                }                
                using (StreamWriter arquivo = File.CreateText(arquivoScript))
                {
                    arquivo.WriteLine($"/* Arquivo gerado em {System.DateTime.Now} */");
                    arquivo.WriteLine();
                    arquivo.Close();
                }
                Console.WriteLine($"Arquivo {nomeTabela}Script criado com sucesso.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ocorreu um erro: {ex.Message}");
            }

            try
            {
                if (File.Exists(feriadosScript))
                {
                    File.Delete(feriadosScript);
                }
                using (StreamWriter arquivo = File.CreateText(feriadosScript))
                {
                    arquivo.WriteLine($"/* Arquivo gerado em {System.DateTime.Now} */");
                    arquivo.WriteLine();
                    arquivo.Close();
                }
                Console.WriteLine($"Arquivo {nomeTabela}Feriados criado com sucesso.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ocorreu um erro: {ex.Message}");
            }
        }

        /// <summary>
        /// Escreve o script de criação da tabela e de inserção dos registros
        /// </summary>
        /// <param name="nomeTabela">Nome da tabela que será criada.</param>
        /// <param name="caminho">Endereço onde o arquivo será salvo.</param>
        /// <param name="primeiroAno">Primeiro ano de registro.</param>
        /// <param name="quantidadeAnos">Quantidade de anos para registro.</param>
        /// <returns>Insere os scripts no arquivo responsável</returns>
        private static void ScriptTabela(string nomeTabela, string caminho, int primeiroAno, int quantidadeAnos)
        {
            StreamWriter arquivo = new(caminho + nomeTabela + "Script.sql", true);
            Calendar calendario = CultureInfo.CurrentCulture.Calendar;
            DateTime dataInicial = new(primeiroAno, 1, 1);
            DateTime dataFinal = dataInicial.AddYears(quantidadeAnos);
            dataFinal = dataFinal.AddDays(-1);

            arquivo.WriteLine("/*");
            arquivo.WriteLine($"Tabela criada com o nome {nomeTabela}");
            arquivo.WriteLine($"Período de {dataInicial.ToString("dd/MM/yyyy")} a {dataFinal.ToString("dd/MM/yyyy")}");
            arquivo.WriteLine($"Sendo portando, um período de {quantidadeAnos}");
            arquivo.WriteLine("*/");
            arquivo.WriteLine();

            arquivo.WriteLine($"IF OBJECT_ID('{nomeTabela}', 'U') IS NOT NULL");
            arquivo.WriteLine("BEGIN");
            arquivo.WriteLine($"DROP TABLE {nomeTabela}");
            arquivo.WriteLine("END");
            arquivo.WriteLine("GO");
            arquivo.WriteLine();

            arquivo.WriteLine($"CREATE TABLE [dbo].[{nomeTabela}] (\n " +
                                $"\tTempoID int,\n" +
                                $"\tData datetime,\n" +
                                $"\tDiaMes int,\n" +
                                $"\tMes int,\n" +
                                $"\tAno int,\n" +
                                $"\tDataCompleta char(10),\n" +
                                $"\tDiaAno int,\n" +
                                $"\tDiaSemana int,\n" +
                                $"\tSemanaMes int,\n" +
                                $"\tSemanaAno int,\n" +
                                $"\tBimestre int,\n" +
                                $"\tTrimestre int,\n" +
                                $"\tSemestre int,\n" +
                                $"\tNomeDiaSemana varchar(20),\n" +
                                $"\tNomeMes varchar(20),\n" +
                                $"\tFeriado bit DEFAULT 0,\n" +
                                $"\tFeriadoNome varchar(200) DEFAULT ''\n" +
                                ")");
            arquivo.WriteLine();

            arquivo.WriteLine($"-- SELECT * FROM [dbo].[{nomeTabela}]");
            arquivo.WriteLine();

            for (int i = 0; dataInicial.AddDays(i) <= dataFinal; i++)
            {
                DateTime data = dataInicial.AddDays(i);
                int diaMes = data.Day;
                int mes = data.Month;
                int ano = data.Year;
                string dataCompleta = ($"{zeroEsquerda(diaMes)}/{zeroEsquerda(mes)}/{ano}");
                int diaAno = data.DayOfYear;
                int diaDaSemana = (int)data.DayOfWeek + 1;
                int semanaDoMes = calendario.GetWeekOfYear(data, CalendarWeekRule.FirstDay, DayOfWeek.Sunday) - calendario.GetWeekOfYear(new DateTime(data.Year, data.Month, 1), CalendarWeekRule.FirstDay, DayOfWeek.Sunday) + 1;
                int semanaDoAno = calendario.GetWeekOfYear(data, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);
                int bimestre = (mes - 1) / 2 + 1;
                int trimestre = (mes - 1) / 3 + 1;
                int semestre = (mes - 1) / 6 + 1;
                string nomeMes = Enum.Meses.GetName(typeof(Enum.Meses), mes);
                string nomeDiaSemana = Enum.DiasDaSemana.GetName(typeof(Enum.DiasDaSemana), diaDaSemana);
                int feriado = 0;
                string feriadoNome = "";

                arquivo.WriteLine($"INSERT INTO [dbo].[{nomeTabela}] " +
                    $"VALUES ({ano}{zeroEsquerda(mes)}{zeroEsquerda(diaMes)}," +
                    $"'{ano}-{zeroEsquerda(mes)}-{zeroEsquerda(diaMes)}T00:00:00'," +
                    $"{diaMes}," +
                    $"{mes}," +
                    $"{ano}," +
                    $"'{dataCompleta}'," +
                    $"{diaAno}," +
                    $"{diaDaSemana}," +
                    $"{semanaDoMes}," +
                    $"{semanaDoAno}," +
                    $"{bimestre}," +
                    $"{trimestre}," +
                    $"{semestre}," +
                    $"'{nomeDiaSemana}'," +
                    $"'{nomeMes}'," +
                    $"{feriado}," +
                    $"'{feriadoNome}')");
            }

            arquivo.WriteLine();
            arquivo.WriteLine($"SELECT * FROM [dbo].[{nomeTabela}]");
            arquivo.Close();


            /// <summary>
            /// Insere um zero(0) à esquerda
            /// </summary>
            /// <param name="num">Valor de entrada.</param>
            /// <returns>String contendo algarismo de 2 dígitos</returns>
            static string zeroEsquerda(int num)
            {
                string valor = Convert.ToString(num);
                if (num < 10)
                {
                    valor = "0" + valor;
                }
                return valor;
            }
        }

        /// <summary>
        /// Escreve o script de atualização da tabela com os feriados
        /// </summary>
        /// <param name="nomeTabela">Nome da tabela que será criada.</param>
        /// <param name="caminho">Endereço onde o arquivo será salvo.</param>
        /// <param name="primeiroAno">Primeiro ano de registro.</param>
        /// <param name="quantidadeAnos">Quantidade de anos para registro.</param>
        /// <returns>Insere os scripts no arquivo responsável</returns>
        private static void BuscarFeriados(string nomeTabela, string caminho, int primeiroAno, int quantidadeAnos)
        {
            StreamWriter arquivo = new(caminho + nomeTabela + "Feriados.sql", true);

            arquivo.WriteLine("/*");
            arquivo.WriteLine($"Tabela criada com o nome {nomeTabela}");
            arquivo.WriteLine($"Feriados de {primeiroAno} a {primeiroAno + quantidadeAnos}");
            arquivo.WriteLine($"Sendo portando, um período de {quantidadeAnos}");
            arquivo.WriteLine("*/");
            arquivo.WriteLine();

            List<Feriado> feriados = new List<Feriado>();

            int anoBusca = primeiroAno;
            string urlApi = "https://brasilapi.com.br/api/feriados/v1/";

            while (anoBusca < primeiroAno + quantidadeAnos)
            {
                try
                {
                    using (HttpClient cliente = new HttpClient())
                    {
                        Task<string> resposta = cliente.GetStringAsync(urlApi + anoBusca);
                        resposta.Wait();
                        List<Feriado> retorno = JsonConvert.DeserializeObject<Feriado[]>(resposta.Result).ToList();

                        foreach (Feriado i in retorno)
                        {
                            feriados.Add(i);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ocorreu um erro: {ex.Message}");
                }
                anoBusca++;
            }

            arquivo.WriteLine("BEGIN TRAN");
            arquivo.WriteLine("-- COMMIT");
            arquivo.WriteLine();

            foreach (Feriado f in feriados)
            {
                string id = f.date.ToString("yyyyMMdd");
                arquivo.WriteLine($"UPDATE [dbo].[{nomeTabela}] SET FeriadoNome = '{f.name}', Feriado = 1 WHERE TempoID = {id}");
            }
            arquivo.WriteLine();
            arquivo.WriteLine($"SELECT * FROM [dbo].[{nomeTabela}] WHERE Feriado = 1");
            arquivo.Close();
        }
    }
}
