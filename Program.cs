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

        Aspirateur aspi = new Aspirateur(0, 0);
        
        env.GenererObstacles();
        env.AfficherEnv();
        Console.ReadKey();
        aspi.capteur.ObserveEnv(env);
        aspi.UpdateMyState();
        aspi.ChooseAnAction();

        // Lance les 2 fils d'execution
        //Task task1 = Task.Factory.StartNew(() => env.runEnv());
        //Task task2 = Task.Factory.StartNew(() => aspi.runAspi(env));

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
                switch ((manoir[j, i].bijoux, manoir[j, i].poussieres))
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

    public void MesurePerformance()
    {
       
        //Performance = Performance + nbCasesPropres - (CompteurElec / nbCasesPropres) - 1.5 * nbBijouxAspires;
        // La formule à modifier peut-être, c'est pour avoir une base
        //return Performance;
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
    public List<Case> Desirs { get { return desirs; } }

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
        // Ajout des nouveaux desirs
        foreach(Case i in capteur.beliefs)
        {
            if(i.bijoux == true || i.poussieres == true)
            {
                desirs.Add(i);
            }
        }        
    }
    public void ChooseAnAction()
    {
        //exploration
        if(desirs.Contains(new Case(posX, posY)))
        {
            Case actualCase = desirs.IndexOf(new Case(posX, posY));
            if(actualCase.poussieres == true)
            {
                Rammasser(actualCase);
            }
            else
            {
                Aspirer(actualCase);
            }
        }
        else
        {
            
            // BFS
            Graph tree = new Graph(posX, posY, desirs);
            Node nexMoove = tree.RemonterArbre();


        }
        Console.WriteLine(tree.solution.valeur);

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
        //performance = env.MesurePerformance();
    }
    
}


class Effecteur
{
    public Effecteur() { } 

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

        // Cn creer les 2 premiers etages car la fonction CreateNodes regarde la variable parent.parent
        Node A = new Node(posX, posY, desirs);   
        if (posX > 0) { A.Enfants.Add(new Node(posX-1, posY, A)); }
        if (posX < 4) { A.Enfants.Add(new Node(posX+1, posY, A)); }
        if (posY > 0) { A.Enfants.Add(new Node(posX, posY-1, A)); }
        if (posY < 4) { A.Enfants.Add(new Node(posX, posY+1, A)); }    
        
        // Verification que la solution n'est pas trouvee
        this.solution = checkSolution(A.Enfants.ToList());
        graph.AddRange(A.Enfants.ToList());
        if(this.solution == null)
        {
            this.solution = BFS(A.Enfants.ToList());
        }

    }

    public Node RemonterArbre()
    {
        Node solu = this.solution;
        // Cette fonction remonte l'arbre
        List<Node> brancheSolution = new List<Node>();
        while(solu != null)
        {
            brancheSolution.Add(solu);
            solu = solu.parent;
        }
        return solu;
    }


    public Node checkSolution(List<Node> etage) 
    {
        // Vérifie si - pour un des nodes de l'etage - toutes les cases ont été visité
        for (int i =0; i<etage.Count; i++)
        {
            if(etage[i].NbCasesAVisiter == 0)
            {
                return etage[i];
            }
        }
        return null;
    }

    /// <summary>
    ///  Cette fonction récursive génère l'arbre étage par étage jusqu'a trouver la solution
    /// </summary>
    public Node BFS(List<Node> etagesup)
    {
        List<Node> etageActuel = new List<Node>();

        // pour chaque noeud de l'etage superieur, on lui ajoute 3 noeuds enfants (pas 4 pour ne pas revenir en arriere)
        for (int i = 0; i < etagesup.Count; i++)
        {
            List<Node> children = CreateChildNodes(etagesup[i]);
            etageActuel.AddRange(children);

            this.solution = checkSolution(children);
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
        parent.Enfants = output;
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
    public int NbCasesAVisiter;
    public List<Case> desirs;

    public List<Node> Enfants { get { return enfants; } set{ this.enfants = value;}}

    //Constructeur 1
    public Node(int x, int y, Node parent)
    {

        int cost_value;//A* ost values (g, h & f)

        this.enfants = new List<Node>();
        this.parent = parent;

        this.valeur = new int[] {x,y};
        this.desirs = parent.desirs;
        
        this.NbCasesAVisiter = parent.NbCasesAVisiter;
        if (desirs.Contains(valeur)) //C'est la fonction qui ne marche pas, ne reconnait pas les paires
        {
            this.NbCasesAVisiter += 1;
        }
    }

    //Constructeur 2
    public Node(int x, int y, List<Case> desirs) //constructeur pour le premier Node, qui n'a pas de parent
    {

        this.enfants = new List<Node>();
        this.parent = null;
        
        this.valeur = new int[] {x,y};
        this.desirs = desirs;

        this.NbCasesAVisiter = desirs.Count;
        if (desirs.Contains(valeur))
        {
            this.NbCasesAVisiter += 1;
        }
    }

    public void PathFunction()
    {
        //will return the path from start to end node that A*
    }
}



