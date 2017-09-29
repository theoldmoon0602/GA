using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyGAEnvironments;

namespace GA1
{
    class Program
    {
        public static void Main(string[] args)
        {
            const int N = 1000;
            const int M = 500;
            GAResult[] results = new GAResult[N];
            for (int i = 0; i < N; i++)
            {
                var ga = new GA(M, 10);
                results[i] = ga.Run();
            }


            double[] fit_avgs = new double[M];
            for (int i = 0; i < M; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    fit_avgs[i] += results[j].fitnesses[i];
                }
                fit_avgs[i] /= N;
            }
            using (var sw = new System.IO.StreamWriter("fitness_avgs.csv"))
            {
                foreach (var avg in fit_avgs)
                {
                    sw.WriteLine("{0},", avg);
                }
            }
            using (var sw = new System.IO.StreamWriter("bestfit_generation.txt"))
            {
                double bestfit_gen = results.Select(x => x.bestfitGen).Average();
                sw.WriteLine(bestfit_gen);
            }
            using (var sw = new System.IO.StreamWriter("sample_fitness.csv"))
            {
                foreach (var fitness in results[0].fitnesses)
                {
                    sw.WriteLine("{0},", fitness);
                }
            }
        }
    }

    class Phenotype
    {
        public string genotype;
        public double fitness;

        public Phenotype(string genotype)
        {
            this.genotype = genotype;
            fitness = 0;
        }
    }

    class GA
    {
        // global random generator
        private Random rd;
        public readonly uint GENELENGTH;
        public readonly double MUTATION_RATE;
        public readonly uint LOOPSIZE;
        public readonly uint GENERATION_SIZE;

        public GA(uint loopsize, uint genelength, double mutation_rate = 0.05, uint generation_size=5)
        {
            GENELENGTH = genelength;
            MUTATION_RATE = mutation_rate;
            LOOPSIZE = loopsize;
            GENERATION_SIZE = generation_size;
        }



        // evaluate indivisuals of genration
        public bool Evaluate(Phenotype[] generation)
        {
            bool bestfit = false;
            foreach (var phenotype in generation)
            {
                double result = Environment1.fitness(phenotype.genotype);
                // may 100
                if (result >= 99.999) { bestfit = true; }
                phenotype.fitness = result;
            }
            return bestfit;
        }
        public Phenotype[] InitialGeneration(uint n)
        {
            Phenotype[] gen = new Phenotype[n];
            for (int i = 0; i < n; i++)
            {
                string genotype = "";
                for (int j = 0; j < GENELENGTH; j++)
                {
                    genotype += rd.Next(2).ToString();
                }
                gen[i] = new Phenotype(genotype);
            }
            return gen;
        }

        public Phenotype[] Selection(Phenotype[] gen)
        {
            // sort generation by fitness
            Array.Sort(gen, delegate (Phenotype p1, Phenotype p2) { return p1.fitness.CompareTo(p2.fitness); });

            // ellite selection
            //return new Phenotype[] { gen[gen.Length - 1], gen[gen.Length - 2] };

            // roulette type selection
            double sum = 0;
            for (int i = 0; i < gen.Length; i++)
            {
                sum += gen[i].fitness;
            }
            Phenotype[] parents = new Phenotype[2];
            int one = -1;
            for (int j = 0; j < 2; j++)
            {
                while (parents[j] is null)
                {
                    int r = rd.Next((int)sum);
                    for (int i = 0; i < gen.Length; i++)
                    {
                        r -= (int)gen[i].fitness;
                        if (r <= 0 && i != one)
                        {
                            parents[j] = gen[i];
                            one = i;
                            break;
                        }
                    }
                }
            }
            return parents;
        }
        public string Crossover(Phenotype[] parents)
        {
            // assertion
            if (parents.Length != 2) { throw new Exception("Internal Error"); }
            if (parents[0] is null) { throw new Exception("Internal Error"); }
            if (parents[1] is null) { throw new Exception("Internal Error"); }

            // uniform crossover algorithm
            // mixing rate is 0.5 (fixval)
            string newC = "";
            for (int i = 0; i < GENELENGTH; i++)
            {
                newC += parents[rd.Next(2)].genotype[i];
            }
            return newC;
        }
        public string Mutation(string genotype)
        {
            char[] xs = genotype.ToCharArray();
            for (int i = 0; i < GENELENGTH; i++)
            {
                if (rd.NextDouble() <= MUTATION_RATE)
                {
                    if (xs[i] == '0') { xs[i] = '1'; }
                    else { xs[i] = '0'; }
                }
            }
            return new string(xs);
        }

        public Phenotype[] NextGeneration(Phenotype[] current)
        {
            Phenotype[] newGen = new Phenotype[current.Length];
            for (int i = 0; i < current.Length; i++)
            {
                Phenotype[] parents = Selection(current);
                string newChromosome = Mutation(Crossover(parents));
                newGen[i] = new Phenotype(newChromosome);
            }
            return newGen;
        }
        public GAResult Run()
        {
            Phenotype[] generation = null;
            rd = new Random(); // initialize seed
            int bestfitGen = -1;
            double[] fitnesses = new double[LOOPSIZE];

            for (int i = 0; i < LOOPSIZE; i++) {
                bool bestfit;
                if (generation is null)
                {
                    generation = InitialGeneration(GENERATION_SIZE);
                }
                else
                {
                    generation = NextGeneration(generation);
                }
                bestfit = Evaluate(generation);
                if (bestfit && bestfitGen == -1) { bestfitGen = i + 1; }
                fitnesses[i] = generation.Select(x => x.fitness).Average();
            }
            if (bestfitGen == -1) { throw new Exception(); }
            return new GAResult(bestfitGen, fitnesses);
        }

    }

    public class GAResult
    {
        public double[] fitnesses;
        public int bestfitGen;

        public GAResult(int bestfitGen, double[] fitnesses)
        {
            this.bestfitGen = bestfitGen;
            this.fitnesses = fitnesses;
        }
    }
}
