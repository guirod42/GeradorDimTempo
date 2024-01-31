using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;

namespace DimTempoGenerator
{
    public class Program
    {
        static void Main()
        {
            int primeiroDia = 1;
            int primeiroMes = 1;
            int primeiroAno = 2000;
            string nomeTabela = "DimTempoTeste";
            int quantidadeAnos = 60;
            ScriptTabela(primeiroDia, primeiroMes, primeiroAno, quantidadeAnos, nomeTabela);

            int[] anosInicialFinal = new int[2] { 2000, 2021 };
            Console.WriteLine("Antes de executar");
            BuscarFeriados();
            Console.WriteLine("Depois de executar");
        }

        static void ScriptTabela(int primeiroDia, int primeiroMes, int primeiroAno, int quantidadeAnos, string nomeTabela)
        {
            int quantidadeDeDias = quantidadeAnos * 366;

            StreamWriter arquivo = new("C:\\ScriptDimTempo.sql", false);
            Calendar calendario = CultureInfo.CurrentCulture.Calendar;

            DateTime dataInicial = new(primeiroAno, primeiroMes, primeiroDia);
            DateTime dataFinal = dataInicial.AddDays(quantidadeDeDias - 1);

            arquivo.WriteLine("/*");
            arquivo.WriteLine($"Arquivo gerado em {System.DateTime.Now}");
            arquivo.WriteLine($"Tabela com o nome {nomeTabela}");
            arquivo.WriteLine($"Data inicial dos valores {dataInicial}");
            arquivo.WriteLine($"Dias criados {quantidadeDeDias}");
            arquivo.WriteLine($"Data final dos valores {dataFinal}");

            arquivo.WriteLine("*/");
            arquivo.WriteLine();

            arquivo.WriteLine($"DROP TABLE [dbo].[{nomeTabela}]");
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
                                $"\tFeriadoNome varchar(20) DEFAULT ''\n" +
                                ")");
            arquivo.WriteLine();

            for (int i = 0; i < quantidadeDeDias; i++)
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

                arquivo.WriteLine($"INSERT INTO [dbo.][{nomeTabela}] " +
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
        static async Task ScriptFeriados(string nomeTabela, int[] anosInicialFinal)
        {
            HttpClient client = new HttpClient { BaseAddress = new Uri("https://brasilapi.com.br/api/feriados/v1") };
            var response = await client.GetAsync("2000");
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Teste aqui ?");

            var resposta = JsonConvert.DeserializeObject(content);
            Console.WriteLine(resposta);
            Console.ReadLine();
        }

        public static void BuscarFeriados()
        {
            string urlApi = "https://brasilapi.com.br/api/feriados/v1/2020";
            try
            {
                using (var cliente = new HttpClient())
                {
                    var resposta = cliente.GetStringAsync(urlApi);
                    resposta.Wait();
                    var retorno = JsonConvert.DeserializeObject<Feriados[]>(resposta.Result).ToList();
                    Console.WriteLine(retorno[0]);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
