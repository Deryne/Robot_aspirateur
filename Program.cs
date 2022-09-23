class Program
    {
        static void Main()
        {
            Console.WriteLine("Creation Environnement vide");
            Environnement env = new Environnement(5,5);

            env.CreerEnvVide();

            Aspirateur aspi = new Aspirateur(0,0);
            Aspirateur.isRunning = true;
            

            //while robot marche

            while (Aspirateur.isRunning)
            {
                Console.WriteLine("Création environnement rempli");
                env.CreerEnv();
                env.AfficherEnv();

                
                //Copie de l'environnement
                aspi.capteur.ObserveEnv(env.manoir);

                //Choisir une action
                //Exploration en BFS pour l'exploration non informé

                //choisir une action
                //Exploration informé




                
                //on arrête le while
                Aspirateur.isRunning = false;

            }


            

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

        public int NbBijoux;//Limite bijoux

        public int NbPoussieres;//Limite poussière

        public Environnement(int bijoux_param, int poussieres_param)
        {
            NbBijoux = bijoux_param;
            NbPoussieres = poussieres_param;
        }

        public void CreerEnvVide()
        {
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                manoir[i,j] = new Case();
                }
            }
        }

        public void CreerEnv()
        {
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {

                Random random = new Random();


                if ((random.Next(100) < 30) && (NbPoussieres > 0))
                {

                   manoir[i,j].poussieres = true; 
                   NbPoussieres = NbPoussieres - 1;
                }

                if ((random.Next(100) < 20) && (NbBijoux > 0))
                {
                   manoir[i,j].bijoux = true; 
                   NbBijoux = NbBijoux - 1;
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
                
                if (manoir[i,j].bijoux == true && manoir[i,j].poussieres == false)
                {
                    Console.Write(" B ");
                }
                if (manoir[i,j].bijoux == true && manoir[i,j].poussieres == true)
                {
                   Console.Write(" X ");
                }

                if (manoir[i,j].bijoux == false && manoir[i,j].poussieres == true)
                {
                   Console.Write(" P ");
                }

                if (manoir[i,j].bijoux == false && manoir[i,j].poussieres == false)
                {
                    Console.Write(" O ");
                }

                }

                Console.Write('\n');

            }
        }


}

class Aspirateur
{
    int posX;
    int posY;

    int unite_elec;//Cout

    public bool exploration_informe; //True pour informé et false pour non informé

    int mesure_perf;

    public static bool isRunning;

    public Case[,] desir = new Case[5, 5];

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

    public void DesirFonction(Case[,] desire)
    {
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                desir[i,j] = new Case();
                desir[i,j].poussieres = false;
                desir[i,j].bijoux = false;
            }
        }
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
        if(posY > 0)
        {
            posY = posY - 1;
            unite_elec = unite_elec - 1;
        }
    }

    public void Down()
    {
        if(posY == 0)
        {
            posY = posY + 1;
            unite_elec = unite_elec - 1;
        }
    }

    public void Left()
    {
        if(posX > 0)
        {
            posX = posX - 1;
            unite_elec = unite_elec - 1;
        }
    }

    public void Right()
    {
        if(posX < 5)
        {
            posX = posX + 1;
            unite_elec = unite_elec - 1;
        }
    }
}


class Capteur
{

    public Case[,] beliefs = new Case[5, 5];

    public Capteur() { }

    public Capteur(Case[,] param_manoir)
    {
        ObserveEnv(param_manoir);
    }
   
    //Beliefs = observation de l'env
    public void ObserveEnv(Case[,] param_manoir)
    {
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                beliefs[i,j] = new Case();
                beliefs[i,j] = param_manoir[i,j];
            }
        }
    }
}


class Effecteur 
{
    public Effecteur() { }
}




