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
            Console.WriteLine("Hello World!");
            StreamWriter arquivo = new StreamWriter("C:\\Stript.txt", true);
            Calendar calendario = CultureInfo.CurrentCulture.Calendar;
            //Write out the numbers 1 to 10 on the same line.


            int primeiroDia = 1;
            int primeiroMes = 1;
            int primeiroAno = 2023;

            int quantidadeDeDias = 365;

            DateTime dataInicial = new DateTime(primeiroAno, primeiroMes, primeiroDia);
            
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
                string feriado = "FALSE";
                string feriadoNome = "";

                arquivo.WriteLine($"INSERT INTO DimTempo VALUES ({ano}{zeroEsquerda(mes)}{zeroEsquerda(diaMes)},\"{data}\",{diaMes},{mes},{ano},\"{dataCompleta}\",{diaAno},{diaDaSemana},{semanaDoMes},{semanaDoAno},{bimestre},{trimestre},{semestre},\"{nomeMes}\",\"{nomeDiaSemana}\",\"{feriado}\",\"{feriadoNome}\")");
            }

            //close the file
            arquivo.Close();

            string zeroEsquerda(int num)
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
