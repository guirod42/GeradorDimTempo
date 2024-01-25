using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;
using System.Globalization;

namespace DimTempoGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            StreamWriter arquivo = new StreamWriter("C:\\ScriptDimTempo.sql", false);
            Calendar calendario = CultureInfo.CurrentCulture.Calendar;

            int primeiroDia = 1;
            int primeiroMes = 1;
            int primeiroAno = 2024;
            string nomeTabela = "DimTempoTeste";

            int quantidadeDeDias = 366;

            DateTime dataInicial = new(primeiroAno, primeiroMes, primeiroDia);
            DateTime dataFinal = dataInicial.AddDays(quantidadeDeDias-1);

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

            arquivo.WriteLine($"CREATE TABLE [dbo].[{nomeTabela}] (");
            arquivo.WriteLine("\tTempoID int,");
            arquivo.WriteLine("\tData datetime,");
            arquivo.WriteLine("\tDiaMes int,");
            arquivo.WriteLine("\tMes int,");
            arquivo.WriteLine("\tAno int,");
            arquivo.WriteLine("\tDataCompleta char(10),");
            arquivo.WriteLine("\tDiaAno int,");
            arquivo.WriteLine("\tDiaSemana int,");
            arquivo.WriteLine("\tSemanaMes int,");
            arquivo.WriteLine("\tSemanaAno int,");
            arquivo.WriteLine("\tBimestre int,");
            arquivo.WriteLine("\tTrimestre int,");
            arquivo.WriteLine("\tSemestre int,");
            arquivo.WriteLine("\tNomeDiaSemana varchar(20),");
            arquivo.WriteLine("\tNomeMes varchar(20),");
            arquivo.WriteLine("\tFeriado bit DEFAULT 0,");
            arquivo.WriteLine("\tFeriadoNome varchar(20) DEFAULT ''");
            arquivo.WriteLine(")");
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

                arquivo.WriteLine($"INSERT INTO {nomeTabela} " +
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

            //close the file
            arquivo.WriteLine();
            arquivo.WriteLine($"SELECT * FROM {nomeTabela}");
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
    }
}
