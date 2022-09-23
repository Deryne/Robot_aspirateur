using System;

class Program
{
    static void Main()
    {
        Console.WriteLine("Creation Environnement");
        Environnement env = new Environnement(5, 5);

        env.CreerEnv();
        env.AfficherEnv();
        env.RunEnv();
    }
}


class Case
{
    //Proprités de la pièce ici appelée "case"
    public bool bijoux; // Il peut y avoir un bijoux dedans
    public bool poussieres; //Il peut y avoir de la poussière dedans (aussi)

    //Définition des coorodnnées dans un constructeur
    public Case()
    {
        bijoux = false;
        poussieres = false;
    }

}


class Environnement
{
    //L'environement est composé de 25 cases avec des propriétés      
    public Case[,] manoir = new Case[5, 5];

    public int NbBijoux; //Limite bijoux

    public int NbBijouxGeneres; // Permet de compter le nombre de bijoux aspirés,
                              // on peut supprimer les 2 variables limites qui ne servent qu'à la création de l'env si ca te vas

    public int NbPoussieres; //Limite poussière

    public double Performance; //Score de performance du robot

    public int CompteurElec; // Compteur electrique, 1 par action du robot

    public Environnement(int bijoux_param, int poussieres_param)
    {
        NbBijoux = bijoux_param;
        NbPoussieres = poussieres_param;
    }

    public void GenerateObstacles(int nombreNouvPoussiere, int nombreNouvBijoux)
    {
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                Random random = new Random();

                if ((random.Next(100) < 30) && (nombreNouvPoussiere > 0))
                {
                    manoir[i, j].poussieres = true;
                    nombreNouvPoussiere -= 1;
                }

                if ((random.Next(100) < 20) && (nombreNouvBijoux > 0))
                {
                    manoir[i, j].bijoux = true;
                    nombreNouvBijoux -= 1;
                    NbBijouxGeneres += 1;
                }
            }
        }
    }

    public void CreerEnv()
    {
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                manoir[i, j] = new Case();
            }
        }
        GenerateObstacles(5, 5);
    }

    public void AfficherEnv()
    {
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                switch ((manoir[i,j].bijoux, manoir[i, j].poussieres))
                {
                    case (true, false):
                        Console.Write(" B ");
                        break;
                    case (true, true):
                        Console.Write(" X ");
                        break;
                    case (false, true):
                        Console.Write(" P ");
                        break;
                    case (false, false):
                        Console.Write(" O ");
                        break;
                }
            }

            Console.WriteLine('\n');

        }
    }

    public void MesurePerformance()
    {
        int nbBijouxPresents = 0;
        int nbCasesPropres = 0;
        for(int i =0; i < 5; i++)
        {
            for (int j=0; j < 5; j++)
            {
                if (manoir[i,j].poussieres == false && manoir[i, j].bijoux == false)
                {
                    nbCasesPropres += 1;
                }
                if(manoir[i,j].bijoux == true)
                {
                    nbBijouxPresents += 1;
                }
            }
        }
        int nbBijouxAspires = NbBijouxGeneres - nbBijouxPresents;
        Performance = Performance + nbCasesPropres - (CompteurElec / nbCasesPropres) - 1.5 * nbBijouxAspires;
        // La formule à modifier peut-être, c'est pour avoir une base
    }
    public void RunEnv()
    {
        while (true)
        {
            Random random = new Random();
            GenerateObstacles(random.Next(3), random.Next(3));
            MesurePerformance();
            AfficherEnv();
            //Thread.Sleep(2000);
            Console.ReadKey();
        }
    }

}
