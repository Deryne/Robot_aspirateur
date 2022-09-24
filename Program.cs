using System.Threading.Tasks;
using System.Threading;
using System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

public class Program
{
    public void Main()
    {
        Console.WriteLine("Creation Environnement vide");
        Environnement env = new Environnement(5, 5);

        Aspirateur aspi = new Aspirateur(0, 0);

        // Lance les 2 fils d'execution
        Task task1 = Task.Factory.StartNew(() => env.runEnv());
        Task task2 = Task.Factory.StartNew(() => aspi.runAspi(env));

        // Attend 20 secondes avant de terminer l'experience
        Thread.Sleep(20 * 1000);

    }
}


class Case
{
    //Proprités de la pièce ici appelée "case"
    public bool bijoux; // Il peut y avoir un bijoux dedans
    public bool poussieres; //Il peut y avoir de la poussière dedans (aussi)

    public double distance; //permet de choisir les cases à visiter en priorite
    public int x; // Permet de calculer la distance, sera utile pour A*
    public int y;

    //Définition des coorodnnées dans un constructeur
    public Case(int xparam, int yparam)
    {
        bijoux = false;
        poussieres = false;

        this.x = xparam;
        this.y = yparam;

    }
}


class Environnement
{
    //L'environement est composé de 25 cases avec des propriétés      
    public Case[,] manoir = new Case[5, 5];

    public int NbBijoux;//Limite bijoux

    public int NbPoussieres;//Limite poussière

    public double Performance; //Score de performance du robot

    public int CompteurElec; // Compteur electrique, 1 par action du robot

    public Environnement(int bijoux_param, int poussieres_param)
    {
        NbBijoux = bijoux_param;
        NbPoussieres = poussieres_param;
        CreerEnvVide();
    }

    public void CreerEnvVide()
    {
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                manoir[i, j] = new Case(i,j);
            }
        }
    }

    public void GenererObstacles()
    {
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {

                Random random = new Random();


                if ((random.Next(100) < 30) && (NbPoussieres > 0))
                {

                    manoir[i, j].poussieres = true;
                    NbPoussieres -= 1;
                }

                if ((random.Next(100) < 20) && (NbBijoux > 0))
                {
                    manoir[i, j].bijoux = true;
                    NbBijoux -= 1;
                }


            }
        }
    }

    public void AfficherEnv()
    {
        Console.WriteLine("Affichage du terrain");

        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                switch ((manoir[i, j].bijoux, manoir[i, j].poussieres))
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
            Console.Write('\n');
        }
    }

    public double MesurePerformance()
    {
       
        Performance = Performance + nbCasesPropres - (CompteurElec / nbCasesPropres) - 1.5 * nbBijouxAspires;
        // La formule à modifier peut-être, c'est pour avoir une base
        return Performance;
    }

    public void runEnv()
    {
        while (true)
        {
            this.GenererObstacles();
            this.AfficherEnv();
            MesurePerformance();
            Thread.Sleep(2000);
        }
    }

}

class Aspirateur
{
    int posX;
    int posY;

    int unite_elec;//Cout

    public bool exploration_informe; //True pour informé et false pour non informé


    public static bool isRunning;

    List<Case> desirs = new List<Case>();

    //Capteurs.
    public Capteur capteur = new Capteur();

    //Effecteurs.
    public Effecteur effecteur = new Effecteur();

    //Constructeur
    public Aspirateur(int param_posX, int paramPosY)
    {
        posX = param_posX;
        posY = paramPosY;
    }

    public void Rammasser(Case param_case)
    {
        param_case.bijoux = false;
        unite_elec = unite_elec - 1;
    }

    public void Aspirer(Case param_case)
    {
        param_case.poussieres = false;
        if (param_case.bijoux)
        {
            param_case.bijoux = false;
            //MALUS ici 
        }
        unite_elec = unite_elec - 1;
    }

    public void Up()
    {
        if (posY > 0)
        {
            posY = posY - 1;
            unite_elec = unite_elec - 1;
        }
    }

    public void Down()
    {
        if (posY == 0)
        {
            posY = posY + 1;
            unite_elec = unite_elec - 1;
        }
    }

    public void Left()
    {
        if (posX > 0)
        {
            posX = posX - 1;
            unite_elec = unite_elec - 1;
        }
    }

    public void Right()
    {
        if (posX < 5)
        {
            posX = posX + 1;
            unite_elec = unite_elec - 1;
        }
    }

    public void runAspi(Environnement env)
    {
        while (true)
        {
            this.capteur.ObserveEnv(env);
            this.UpdateMyState();
            this.ChooseAnAction();
            Thread.Sleep(2000);
        }
    }

    public void UpdateMyState() // Mets à jour les desires
    {
        foreach(Case i in desirs) 
        {
            i.distance = Math.Sqrt(Math.Pow(i.x - this.posX, 2) + Math.Pow(i.y - posY, 2));
        }
        desirs = desirs.OrderBy(o => o.distance).ToList();

    }
    public void ChooseAnAction()
    {
        //exploration
    }
}


class Capteur
{

    public Case[,] beliefs = new Case[5, 5];
    public double performance;
    public Capteur() { }

    public Capteur(Environnement env)
    {
        ObserveEnv(env);
    }

    //Beliefs = observation de l'env
    public void ObserveEnv(Environnement env)
    {
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                beliefs[i, j] = new Case(i,j);         //Pourquoi créer une instance de case() si on ne l'utilise pas ? 
                beliefs[i, j] = env.manoir[i, j];
            }
        }
        performance = env.MesurePerformance();
    }
    
}


class Effecteur
{
    public Effecteur() { } // Est ce qu'il ne faudrait pas mettre les methodes up, droite, gauche, etc dans cette classe?
}




