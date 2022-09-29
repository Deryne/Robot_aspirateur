using System.Threading.Tasks;
using System.Threading;
using System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

public class Program
{
    static void Main()
    {
        Console.WriteLine("Creation Environnement vide");
        Environnement env = new Environnement(5, 5);
        env.GenererObstacles();
        Aspirateur aspi = new Aspirateur(0, 0);
        
        /*while (true)
        {
            env.GenererObstacles();        
            env.AfficherEnv();
            Console.WriteLine(aspi.posX.ToString()+","+aspi.posY.ToString());
            Console.ReadKey();
            aspi.capteur.ObserveEnv(env);
            aspi.UpdateMyState();
            aspi.ChooseAnAction();
            
        }*/
     
        // Lance les 2 fils d'execution
        Task task1 = Task.Factory.StartNew(() => env.runEnv(aspi));
        Task task2 = Task.Factory.StartNew(() => aspi.runAspi(env));

        // Attend 20 secondes avant de terminer l'experience
        Thread.Sleep(20 * 1000);
        Console.ReadKey();
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
    public int NbBijoux;
    public int NbPoussieres;
    public double Performance; //Score de performance du robot
    //public Aspirateur asp;
    //public Aspirateur Asp { get { return this.asp;} set { this.asp = value;} }

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

                if ((random.Next(100) < 2) && (NbPoussieres < 25))
                {
                    if(manoir[i, j].poussieres == false) 
                    { 
                        manoir[i, j].poussieres = true;
                        NbPoussieres += 1;
                    }
                }
                if ((random.Next(100) < 2) && (NbBijoux < 25))
                {
                    if (manoir[i,j].bijoux == false)
                    {
                        manoir[i, j].bijoux = true;
                        NbBijoux += 1;
                    }   
                }
            }
        }
    }

    public void AfficherEnv(Aspirateur asp)
    {
        Console.WriteLine("Affichage du terrain");
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {   
                if(asp.posX == j && asp.posY == i) // Si c'est la case de l'aspirateur
                {
                    Console.BackgroundColor = ConsoleColor.Red;
                }
                switch ((manoir[j, i].bijoux, manoir[j, i].poussieres))
                {
                    case (true, false):
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.Write(" B ");
                        break;
                    case (true, true):
                         Console.ForegroundColor = ConsoleColor.Green;
                         Console.Write(" X ");
                         break;
                    case (false, true):
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write(" P ");
                        break;
                    case (false, false):
                        Console.Write(" O ");
                        break;
                }
                Console.BackgroundColor = ConsoleColor.Black; 
                Console.ForegroundColor = ConsoleColor.White;
                
            }
            Console.WriteLine();
        }
    }

    public void MesurePerformance()
    { 
        //Performance = Performance + nbCasesPropres - (CompteurElec / nbCasesPropres) - 1.5 * nbBijouxAspires;
        // La formule à modifier peut-être, c'est pour avoir une base
        //return Performance;
    }

    public void runEnv(Aspirateur asp)
    {
        while (true)
        {
            this.GenererObstacles();
            MesurePerformance();
            Thread.Sleep(2000);
        }
    }
}

class Aspirateur
{
    public int posX;
    public int posY;
    int compteur_elec;  //Cout
    public bool exploration_informe; //True pour informé et false pour non informé
    public static bool isRunning;
    List<Case> desirs = new List<Case>();
    public List<Case> Desirs { get { return desirs; } }
    public Capteur capteur = new Capteur();
    List<Node> plan;

    public Aspirateur(int param_posX, int paramPosY)
    {
        posX = param_posX;
        posY = paramPosY;
    }

    public void Rammasser(Case param_case)
    {
        param_case.bijoux = false;
        compteur_elec -= 1;
        Console.WriteLine("-----------------------------------------Ramasser");
    }
    public void Aspirer(Case param_case)
    {
        param_case.poussieres = false;
        if (param_case.bijoux)
        {
            param_case.bijoux = false;
            //MALUS ici 
        }
        compteur_elec -= 1;
        Console.WriteLine("-----------------------------------------Aspirer");
    }
    public void Up()
    {
        if (posY > 0)
        {
            posY -= 1;
            compteur_elec -= 1;
        }
    }
    public void Down()
    {
        if (posY < 5)
        {
            posY += 1;
            compteur_elec -= 1;
        }
    }
    public void Left()
    {
        if (posX > 0)
        {
            posX -= 1;
            compteur_elec -= 1;
        }
    }
    public void Right()
    {
        if (posX < 5)
        {
            posX += 1;
            compteur_elec -= 1;
        }
    }

    public void runAspi(Environnement env)
    {
        while (true)
        {
            env.AfficherEnv(this);
            this.capteur.ObserveEnv(env);
            bool manoirChanged = this.UpdateMyState();
            this.ChooseAnAction(manoirChanged);
            Thread.Sleep(1500);
        }
    }

    public bool UpdateMyState() // Mets à jour les desires
    {
        bool manoirChanged = true;
        List<Case> desirsTemp = desirs.ToList();
        desirs = new List<Case>();
        foreach(Case i in capteur.beliefs)
        {
            if(i.bijoux == true || i.poussieres == true)
            {
                desirs.Add(i);
            }
        }     
        if(desirsTemp == desirs) { manoirChanged = false;}
        return manoirChanged;
    }
    public void ChooseAnAction(bool manoirChanged)
    {
        // Verifie si le robot se trouve sur un desire
        int index  = - 1;
        for (int i = 0; i < desirs.Count; i++)
        {
            if (desirs[i].x == posX && desirs[i].y == posY)
            {
                index = i; break;
            }
        }
        if(index != -1)
        {
            Case actualCase = desirs[index];
            if(actualCase.bijoux == true)
            {
                Rammasser(actualCase);
            }
            else
            {
                Aspirer(actualCase);
            }
        }
        else // Exploration
        {
            // BFS
            if(manoirChanged == true)
            {   
                Graph tree = new Graph(posX, posY, desirs);  // génère le graph en BFS et trouve la solution
                plan = tree.RemonterArbre();  // remonte la solution pour trouver le prochain mouvement
            }
            Node nexMoove = plan[plan.Count-1];
            plan.Remove(nexMoove);

            if(posX == nexMoove.valeur[0]) //ca veut dire que le mouvement est dans la dimension y
            {
                if(posY + 1 == nexMoove.valeur[1]) 
                {
                    Down();
                }
                else
                {
                    Up();
                }
            }
            else //mouvement dimension x
            {
                if(posX + 1 == nexMoove.valeur[0])
                {
                    Right();
                }
                else
                {
                    Left();
                }
            }
        }
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

    public void ObserveEnv(Environnement env)
    {
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                beliefs[i, j] = env.manoir[i, j];
            }
        }
        //performance = env.MesurePerformance();
    }
}




/// <summary>
///  Cette classe permet d'appliquer plusieur aglorithmes d'exploration à notre champs de recherche
///  Au lieu de créer un graph puis de le parcourir on l'explore en même temps qu'on le créer
///  Je n'ai pas pris en compte aspirer+ramasser, voir si c'est utile de les intégrer ou alors si ca complexifie pour rien
/// </summary>
class Graph
{
    //solution cherchée : manoir propre --> le robot passe sur toutes les cases 
    public List<Node> graph; //liste de nodes reliés par leur attributs parent et enfants
    public List<Case> desirs;  // liste des cases à visiter
    public Node solution;   // Noeud solution trouvé, il suffit ensuite de remonter le chemin grace aux attributs 

    public Graph(int posX, int posY, List<Case> desirsparam)
    {
        this.graph = new List<Node>();
        this.solution = null;
        this.desirs = desirsparam;

        // On creer les 2 premiers etages car la fonction CreateNodes regarde la variable parent.parent
        Node A = new Node(posX, posY, desirs);   
        if (posX > 0) { A.enfants.Add(new Node(posX-1, posY, A)); }
        if (posX < 4) { A.enfants.Add(new Node(posX+1, posY, A)); }
        if (posY > 0) { A.enfants.Add(new Node(posX, posY-1, A)); }
        if (posY < 4) { A.enfants.Add(new Node(posX, posY+1, A)); }    
        
        // Verification que la solution n'est pas trouvee
        this.solution = checkSolution(A.enfants.ToList());
        graph.Add(A);
        graph.AddRange(A.enfants.ToList());
        if(this.solution == null)
        {
            this.solution = BFS(A.enfants.ToList());
        }
    }

    public List<Node> RemonterArbre()
    {
        Node solu = this.solution;
        List<Node> brancheSolution = new List<Node> { solu};
        while(solu.parent.parent != null)
        {
            solu = solu.parent;
            brancheSolution.Add(solu);
        }
        Console.WriteLine("Chemin trouve");
        AfficherEtage(brancheSolution);
        return brancheSolution;
    }
    public void AfficherEtage(List<Node> etage)
    {
        string str = "";
        foreach(Node n in etage)
        {
             str += "  " + n.valeur[0].ToString() + "," + n.valeur[1].ToString();
        }
        Console.WriteLine(str);
        Console.WriteLine();
    }
    public Node checkSolution(List<Node> etage) 
    {
        // Vérifie si - pour un des nodes de l'etage - toutes les cases ont été visité
        for (int i =0; i<etage.Count; i++)
        {
            if(etage[i].desirs.Count == 0)
            {
                return etage[i];
            }
        }
        return null;
    }

    /// <summary>
    ///  Cette fonction récursive génère l'arbre étage par étage jusqu'a trouver la solution
    ///  Je pense que ne créer que 3 noeuds permet de gagner du temps d'execution par rapport au détour que ca fait faire
    /// </summary>
    public Node BFS(List<Node> etagesup)
    {
        List<Node> etageActuel = new List<Node>();

        // pour chaque noeud de l'etage superieur, on lui ajoute 3 noeuds enfants (pas 4 pour ne pas revenir en arriere)
        for (int i = 0; i < etagesup.Count; i++)
        {
            if (this.solution == null)
            {
                List<Node> children = CreateChildNodes(etagesup[i]);
                etageActuel.AddRange(children);

                this.solution = checkSolution(children);
            }            
        }
        graph.AddRange(etageActuel);

        //si la solution n'est pas trouvee on passe a l'etage suivant
        if(this.solution == null)
        {
            return BFS(etageActuel);
        }
        else { return this.solution; }
    }

    public List<Node> CreateChildNodes(Node parent)
    {
        // Créer 3 Nodes enfants, prend en compte les sorties d'espace de recherche
        int x = parent.valeur[0];
        int y = parent.valeur[1];
        List<Node> output = new List<Node>();
        if (parent.parent.valeur[0] == x - 1)
        {
            if (x < 4) { output.Add( new Node(x+1, y, parent)); }
            if (y < 4) { output.Add( new Node(x, y+1, parent)); }
            if (y > 0) { output.Add( new Node(x, y-1, parent)); }
        }
        else if (parent.parent.valeur[0] == x + 1)
        {
            if (x > 0) { output.Add( new Node(x-1, y, parent)); }
            if (y < 4) { output.Add( new Node(x, y+1, parent)); }
            if (y > 0) { output.Add( new Node(x, y-1, parent)); }
        }
        else if (parent.parent.valeur[1] == y - 1)
        {
            if (x > 0) { output.Add( new Node(x-1, y, parent)); }
            if (x < 4) { output.Add( new Node(x+1, y, parent)); }
            if (y < 4) { output.Add( new Node(x, y+1, parent)); }
        }
        else if (parent.parent.valeur[1] == y + 1)
        {
            if (x > 0) { output.Add( new Node(x-1, y, parent)); }
            if (x < 4) { output.Add( new Node(x+1, y, parent)); }
            if (y > 0) { output.Add( new Node(x, y-1, parent)); }
        }
        
        parent.enfants = output;
        return output;
    }

}

// Comporte un compteur du nombre de cases qu'il reste à visiter, variable verifiee à la construction
// Comporte le noeud parent et une liste des noeuds enfants
class Node
{
    public List<Node> enfants;
    public Node parent;
    public int[] valeur;
    public List<Case> desirs;

    public Node(int x, int y, Node parent)
    {
        this.enfants = new List<Node>();
        this.parent = parent;

        this.valeur = new int[] {x,y};
        this.desirs = parent.desirs.ToList();

        int index = desirCheck();  // si le noeud créer fait parti des désirs, on le retire de la liste
        if (index!=-1)
        {
            desirs.RemoveAt(index);
        }
    }

    public Node(int x, int y, List<Case> desirs) //constructeur pour le premier Node, qui n'a pas de parent
    {
        this.enfants = new List<Node>();
        this.parent = null;
        
        this.valeur = new int[] {x,y};
        this.desirs = desirs;

        int index = desirCheck();
        if (index!=-1)
        {
            desirs.RemoveAt(index);
        }
    }

    public int desirCheck() //permet de trouver le desir concerné
    {
        int index  = - 1;
        for (int i = 0; i < desirs.Count; i++)
        {
            if (desirs[i].x == valeur[0] && desirs[i].y == valeur[1])
            {
                index = i; break;
            }
        }
        return index;
    }
}
