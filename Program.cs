

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Creation Environnement");
            Environment env = new Environment(5,5);
            

        }
    }


class Case
{
        //Proprités de la pièce ici appelée "case"
        public bool bijoux = false; // Il peut y avoir un bijoux dedans
        public bool poussiere = false; //Il peut y avoir de la poussière dedans (aussi)

        //Définition des coorodnnées dans un constructeur
        public Case(){}

        //Ajout d'un bijox à la case
        public void SetBijoux()
        {
            bijoux = true;
        }

        //Ajout de poussière à la case
        public void SetPoussiere()
        {
            poussiere = true;
        }
}


class Environnement
{
        //L'environement est composé de 25 cases avec des propriétés      
        public Case[,] manoir = new Case[5,5];

        public int NbBijoux;

        public int NbPoussieres;

        public Environnement(int bijoux_param, int poussieres_param)
        {
            NbBijoux = bijoux_param;
            NbPoussieres = poussieres_param;

            CreerEnv();

        }


        public void CreerEnv()
        {
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    manoir[i,j].SetBijoux();
                    
                }
            }
        }


}





