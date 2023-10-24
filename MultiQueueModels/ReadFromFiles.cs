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

        public int CalcWaitingTime(SimulationSystem sys)
        {
            //calculate the total waiting time for each case in simulation table
            int total_time = 0;
           for(int i = 0; i < sys.SimulationTable.Count; i++)
            {
                SimulationCase sim_case = sys.SimulationTable[i];
                total_time += sim_case.TimeInQueue;
            }
            return total_time;
        }
        
        public decimal CalcAvrWaitingTime(SimulationSystem sys)
        {
            decimal total_waitingtime = CalcWaitingTime(sys);
            //avr = total waiting time/total no of customers
            decimal avr_waitingtime = total_waitingtime / (sys.SimulationTable.Count);
            return avr_waitingtime;
        }

        public int CalcNoOfWaitedCustomers(SimulationSystem sys)
        {
            int noOfCustomers = 0;
            for(int i = 0; i < sys.SimulationTable.Count; i++)
            {
                SimulationCase sim_case = sys.SimulationTable[i];
                if (sim_case.TimeInQueue > 0)
                    noOfCustomers++;
            }
            return noOfCustomers;
        }

        public decimal CalcProbability_wait(SimulationSystem sys)
        {
            int noOfCustomers = CalcNoOfWaitedCustomers(sys);
            //prob(wait) = no of waited customers / total no of customers
            decimal probability_wait = (decimal)noOfCustomers / sys.SimulationTable.Count;
            return probability_wait;
        }

        void PerformanceMeasuresPerServer(SimulationSystem sys)
        {
               //total service time of each server
            for(int i = 0; i < sys.SimulationTable.Count; i++)
            {
                if (sys.SimulationTable[i].EndTime > sys.total_runtime)
                    sys.total_runtime = sys.SimulationTable[i].EndTime;
            }
               //total idle time of each server
            for (int i = 0; i < sys.Servers.Count; i++)
            {
                decimal total_workingtime = sys.Servers[i].TotalWorkingTime;
                decimal total_idletime = sys.total_runtime - total_workingtime;
                //IdleProbability = total idle time of server i /total run time 
                sys.Servers[i].IdleProbability = total_idletime / sys.total_runtime;

                //average service time = total service time / total no of customers
                int noOfCustomers = sys.Servers[i].TotalNoOfCustomers.Count;
                decimal avr_servicetime = 0;
                if (noOfCustomers > 0)
                {
                    avr_servicetime = total_workingtime / noOfCustomers;
                }
                sys.Servers[i].AverageServiceTime = avr_servicetime;

                //utilization(i) = total time server i spends on calls / total run time of simulation
                sys.Servers[i].Utilization = sys.Servers[i].TotalWorkingTime / sys.total_runtime;
            }

        }

    }
        
}
