class Program
    {
        static void Main()
        {
            Console.WriteLine("Creation Environnement");
            Environnement env = new Environnement(5,5);

            env.CreerEnv();
            env.AfficherEnv();
            

        }
}


class Case
{
        //Proprités de la pièce ici appelée "case"
        public bool bijoux; // Il peut y avoir un bijoux dedans
        public bool poussieres; //Il peut y avoir de la poussière dedans (aussi)

        //Définition des coorodnnées dans un constructeur
        public Case(){
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


        public void CreerEnv()
        {
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                manoir[i,j] = new Case();

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
            

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                
                if (manoir[i,j].bijoux == true && manoir[i,j].poussieres == false)
                {
                    Console.WriteLine(" B ");
                }
                if (manoir[i,j].bijoux == true && manoir[i,j].poussieres == true)
                {
                   Console.WriteLine(" X ");
                }

                if (manoir[i,j].bijoux == false && manoir[i,j].poussieres == true)
                {
                   Console.WriteLine(" P ");
                }

                if (manoir[i,j].bijoux == false && manoir[i,j].poussieres == false)
                {
                    Console.WriteLine(" O ");
                }

                }

                Console.WriteLine('\n');

            }
        }


}





