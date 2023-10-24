using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace MultiQueueModels
{
    public class ReadFromFiles
    {

        public void loadfortest(SimulationSystem sys, string path)
        {
            string[] fileContent = File.ReadAllLines(path);
            TimeDistribution timeDistribution;
            TimeDistribution servicetable;
            sys.NumberOfServers = int.Parse(fileContent[1]);
            sys.StoppingNumber = int.Parse(fileContent[4]);

            if (fileContent[7] == "1")
            {
                sys.StoppingCriteria = Enums.StoppingCriteria.NumberOfCustomers;
            }
            else if (fileContent[7] == "2")
            {
                sys.StoppingCriteria = Enums.StoppingCriteria.SimulationEndTime;
            }

            if (fileContent[10] == "1")
            {
                sys.SelectionMethod = Enums.SelectionMethod.HighestPriority;
            }
            else if (fileContent[10] == "2")
            {
                sys.SelectionMethod = Enums.SelectionMethod.LeastUtilization;
            }
            else
            {
                sys.SelectionMethod = Enums.SelectionMethod.Random;
            }

            /*---------------------------------------*/
            // calculate the interarrival-time distribution table
            int val = 13;
            int serversIndex = 0;

            while (val < fileContent.Length && fileContent[val] != "")
            {
                string[] parts = fileContent[val].Split(',');

                for (int i = 0; i < parts.Length; i += 2)
                {
                    timeDistribution = new TimeDistribution();
                    if (i + 1 < parts.Length)
                    {
                        timeDistribution.Time = int.Parse(parts[i]);
                        timeDistribution.Probability = decimal.Parse(parts[i + 1]);
                    }
                    sys.InterarrivalDistribution.Add(timeDistribution);
                }
                
                val++;
                serversIndex = val + 2;
                
            }
            cummprobability(sys.InterarrivalDistribution);
            /*---------------------------------------*/

            for (int i = 0; i < sys.NumberOfServers; i++)
            {
                Server server = new Server();

                server.ID = i + 1;

                while (serversIndex < fileContent.Length && fileContent[serversIndex] != "")
                {
                    servicetable = new TimeDistribution();

                    string[] parts = fileContent[serversIndex].Split(',');

                    servicetable.Time = int.Parse(parts[0]);
                    servicetable.Probability = decimal.Parse(parts[1]);

                    server.TimeDistribution.Add(servicetable);

                    serversIndex++;
                }
                serversIndex += 2;

                sys.Servers.Add(server);
            }

            //foreach (var item in sys.InterarrivalDistribution)
            //{
            //    Console.WriteLine($"Time: {item.Time}, Probability: {item.Probability}, Cummulative Probability: {item.CummProbability}");
            //}

            //Console.WriteLine("******************************************");
            foreach (var server in sys.Servers)
            {
                cummprobability(server.TimeDistribution);

                //for (int i = 0; i < server.TimeDistribution.Count; i++)
                //{
                //    Console.WriteLine($"Time:{server.TimeDistribution.ElementAt(i).Time}, Probability:{server.TimeDistribution.ElementAt(i).Probability}, Cummulative Probability: {server.TimeDistribution.ElementAt(i).CummProbability}");
                //}
                //Console.WriteLine("************************");

            }
        }

        public List<TimeDistribution> cummprobability(List<TimeDistribution> timeDistributions)
        {
            for (int i = 0; i < timeDistributions.Count; i++)
            {
                if (i == 0)
                {
                    timeDistributions[i].CummProbability = timeDistributions[i].Probability;
                    timeDistributions[i].MinRange = 0;
                    timeDistributions[i].MaxRange = (int)(timeDistributions[i].CummProbability * 100);
                }
                else
                {
                    timeDistributions[i].CummProbability = timeDistributions[i - 1].CummProbability + timeDistributions[i].Probability;
                    timeDistributions[i].MinRange = (int)(timeDistributions[i - 1].CummProbability * 100) + 1;
                    timeDistributions[i].MaxRange = (int)(timeDistributions[i].CummProbability * 100);
                }
            }
            return timeDistributions;
        }

        public int RandomValues(List<TimeDistribution> timeDistributions, int random)
        {
            int serviceTime = -1;
            for (int i = 0; i < timeDistributions.Count; i++)
            {
                if (random >= timeDistributions[i].MinRange && random <= timeDistributions[i].MaxRange)
                { 
                    serviceTime = timeDistributions[i].Time;
                }
            }
            return serviceTime;
        }

        
        

    }
}
