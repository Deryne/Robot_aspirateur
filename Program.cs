//Importations
using System.Threading.Tasks;
using System.Threading;
using System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

public class Program
{
    //Programme principal
    static void Main()
    {
        //Initialisation d'un environnement vide
        Environnement env = new Environnement(5, 5);
        env.GenererObstacles();

        //Création d'un robot aspirateur
        Aspirateur aspi = new Aspirateur(0, 0, false);
        
        //Pendant que le robot fonctionne
        while (true)
        {
            //Génération de premières poussières et bijoux dans l'environnement
            env.GenererObstacles();
            //Affichage de l'environnement 
            env.AfficherEnv(aspi);
            //Affichage de la position du robot
            Console.WriteLine(aspi.posX.ToString()+","+aspi.posY.ToString());
            Console.ReadKey();
            //Création d'une copie de l'environnement pour le robot
            aspi.capteur.ObserveEnv(env);
            //
            bool manoirChanged = aspi.UpdateMyState();
            //
            aspi.ChooseAnAction(manoirChanged);
            //Just do it
            
        }
        
     /*
        // Lance les 2 fils d'execution
        Task task1 = Task.Factory.StartNew(() => env.runEnv(aspi));
        Task task2 = Task.Factory.StartNew(() => aspi.runAspi(env));

        // Attend 20 secondes avant de terminer l'experience
        Thread.Sleep(20 * 1000);*/
        Console.ReadKey();
    }
}

//Classe Case
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

//Classe environnement
class Environnement
{
    //L'environement est composé de 25 cases avec des propriétés      
    public Case[,] manoir = new Case[5, 5];
    //Possède un nombre limmite de bijoux dedans
    public int NbBijoux;
    //Possède un nombre limite de poussières dedans
    public int NbPoussieres;
    //Score de performance du robot
    public double Performance; 
    //public Aspirateur asp;
    //public Aspirateur Asp { get { return this.asp;} set { this.asp = value;} }

    //Constructeur
    public Environnement(int bijoux_param, int poussieres_param)
    {
        NbBijoux = bijoux_param;
        NbPoussieres = poussieres_param;
        //Création d'un manoir vide
        CreerEnvVide();
    }

    //Fonction qui permet d'initialiser le manoir
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

    //Fonction qui génère les poussières et les bijoux
    public void GenererObstacles()
    {
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                Random random = new Random();//Nombre random

                //Chance de 2% d'avoir de la poussière
                if ((random.Next(100) < 2) && (NbPoussieres < 25))
                {
                    if(manoir[i, j].poussieres == false) 
                    { 
                        manoir[i, j].poussieres = true;
                        NbPoussieres += 1;//Une poussière de plus dans le manoir à aspirer
                    }
                }
                //Chance de 2% d'avoir un bijoux
                if ((random.Next(100) < 2) && (NbBijoux < 25))
                {
                    if (manoir[i,j].bijoux == false)
                    {
                        manoir[i, j].bijoux = true;
                        NbBijoux += 1;//Un bijoux de plus à ramasser
                    }   
                }
            }
        }
    }

    //Fonction qui affiche l'environnement en temps réel
    public void AfficherEnv(Aspirateur asp)
    {
        //Console.WriteLine("Affichage du terrain");
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
                    case (true, false): //Si la case possède un bijoux et pas de poussières
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.Write(" B ");
                        break;
                    case (true, true): //Si la case possède un bijoux et une poussière
                         Console.ForegroundColor = ConsoleColor.Green;
                         Console.Write(" X ");
                         break;
                    case (false, true): //Si la case possède de la poussière
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write(" P ");
                        break;
                    case (false, false): //Si la case ne contient rien
                        Console.Write(" O ");
                        break;
                }
                Console.BackgroundColor = ConsoleColor.Black; 
                Console.ForegroundColor = ConsoleColor.White;
                
            }
            Console.WriteLine();
        }
    }

    //Mesure de la performance pour l'apprentissage
    public void MesurePerformance()
    { 
        //Performance = Performance + nbCasesPropres - (CompteurElec / nbCasesPropres) - 1.5 * nbBijouxAspires;
        // La formule à modifier peut-être, c'est pour avoir une base
        //return Performance;
    }

    //Thread de l'environnement
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

//Classe aspirateur
class Aspirateur
{
    public int posX; //Position en ligne
    public int posY; //Position en colonne
    public int compteur_elec;//Cout à chasue action
    public bool exploration_informe;//True pour ida* et false pour bfs
    //public bool isRunning;
    List<Case> desirs = new List<Case>();
    public List<Case> Desirs { get { return desirs; } }
    public Capteur capteur = new Capteur();//Création d'un capteur pour observer l'environnement
    List<Node> plan;//Plan d'actions de l'aspirateir
    public Effecteur effector = new Effecteur();//Création d'un effecteur pour agir

    //Constructeur
    public Aspirateur(int param_posX, int paramPosY, bool exploration_informe)
    {
        posX = param_posX;//Position X
        posY = paramPosY;//Position Y
        this.exploration_informe = exploration_informe; //Type d'exploration
    }

    //Thread de l'aspirateur
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

    // Mets à jour les desirs (intentions)
    public bool UpdateMyState() 
    {
        bool manoirChanged = true;//
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

    //Choix de l'action à effectuer
    public void ChooseAnAction(bool manoirChanged)
    {
        // Verifie si le robot se trouve sur un desir
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
                effector.Rammasser(this,  actualCase);
            }
            else
            {
                effector.Aspirer(this, actualCase   );
            }
        }
        else // Exploration
        {
            // BFS
            if(manoirChanged == true)
            {   
                Graph tree = new Graph(exploration_informe, posX, posY, desirs);  // génère le graph en BFS et trouve la solution
                plan = tree.RemonterArbre();  // remonte la solution pour trouver le prochain mouvement
            }
            Node nexMoove = plan[plan.Count-1];
            plan.Remove(nexMoove);

            if(posX == nexMoove.valeur[0]) //ca veut dire que le mouvement est dans la dimension y
            {
                if(posY + 1 == nexMoove.valeur[1]) 
                {
                    effector.Down(this);
                }
                else
                {
                    effector.Up(this);
                }
            }
            else //mouvement dimension x
            {
                if(posX + 1 == nexMoove.valeur[0])
                {
                    effector.Right(this);
                }
                else
                {
                    effector.Left(this);
                }
            }
        }
    }
}

//Classe capteur
class Capteur
{
    //création d'une vision vide du manoir
    public Case[,] beliefs = new Case[5, 5];
    public double performance;

    //Constrcteur
    public Capteur() { }

    //Constructeur
    public Capteur(Environnement env)
    {
        ObserveEnv(env);
    }

    //Fonction qui crée une copie du manoir à un instant t
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

//Classe Effecteur : permet au robot aspirateur d'effectuer des actions
class Effecteur
{
    //Constructeur vide
    public Effecteur(){ }

    //Fonction aller à gauche
    public void Left(Aspirateur aspi)
    {
        //Vérifie si le robot n'est pas sur la première colonne
        if (aspi.posX > 0)
        {
            aspi.posX -= 1;
            aspi.compteur_elec -= 1;
        }
    }

    //Fonction aller à droite
    public void Right(Aspirateur aspi)
    {
        //Vérifie si le robot n'est pas sur la dernière colonne
        if (aspi.posX < 4)
        {
            aspi.posX += 1;
            aspi.compteur_elec -= 1;
        }
    }

    //Fonction aller en haut
    public void Up(Aspirateur aspi)
    {
        //Vérifie si le robot n'est pas sur la première ligne
        if (aspi.posY > 0)
        {
            aspi.posY -= 1;
            aspi.compteur_elec -= 1;
        }
    }

    //Fonction aller en bas
    public void Down(Aspirateur aspi)
    {
        //Vérifie si le robot n'est pas sur la dernière ligne
        if (aspi.posY < 4)
        {
            aspi.posY += 1;
            aspi.compteur_elec -= 1;
        }
    }

    //Fonction ramasser
    public void Rammasser(Aspirateur aspi, Case param_case)
    {
        param_case.bijoux = false;
        aspi.compteur_elec -= 1;
        Console.WriteLine("-----------------------------------------Ramasser");
    }
    public void Aspirer(Aspirateur aspi, Case param_case)
    {
        param_case.poussieres = false;
        if (param_case.bijoux)
        {
            param_case.bijoux = false;
            //MALUS ici 
        }
        aspi.compteur_elec -= 1;
        Console.WriteLine("-----------------------------------------Aspirer");
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

    public Graph(bool exploration_informe, int posX, int posY, List<Case> desirsparam)
    {
        this.graph = new List<Node>();
        this.solution = null;
        this.desirs = desirsparam;

        // On creer les 2 premiers etages car la fonction CreateNodes regarde la variable parent.parent
        Node A = new Node(posX, posY, desirs);  
        /*
        if (posX > 0) { A.enfants.Add(new Node(posX-1, posY, A)); }
        if (posX < 4) { A.enfants.Add(new Node(posX+1, posY, A)); }
        if (posY > 0) { A.enfants.Add(new Node(posX, posY-1, A)); }
        if (posY < 4) { A.enfants.Add(new Node(posX, posY+1, A)); }    */
        
        // Verification que la solution n'est pas trouvee
        this.solution = checkSolution(A.enfants.ToList());
        graph.Add(A);
        graph.AddRange(A.enfants.ToList());
        if(this.solution == null)
        {
            //Si l'aspirateur explore en BFS
            if(exploration_informe == false)
            {
                this.solution = BFS(A.enfants.ToList());
            }
            //Si l'aaspirateur explore en A*
            else
            {
                Dictionary<Node, double> temp = new Dictionary<Node, double> ();
                temp.Add(A, 10000);
                this.solution = IDA(A, temp);
            }
        }
    }


    public Node IDA(Node actualNode, Dictionary<Node, double> savedNodes)
    {
        // create nodes
        // check if child solution
        // calculate heur f pour chaque child
        // add child to savednodes
        // remove actualnode from savednodes
        // tri local des childs par f
        // if(mininmum f des child >= min savednodes) : 
        //              Go to min f des savednodes ordonné par le plus ancien
        // else:
        //              foreach f :
        //                  explore node
        List<Node> child = CreateChildNodes(actualNode);
        this.solution = checkSolution(child);
        for(int i=0; i<child.Count; i++)
        {
            double f = g(child[i]) + heuristique(child[i]);
            savedNodes.Add(child[i], f);
        }
        savedNodes.Remove(actualNode);
        savedNodes = savedNodes.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
        if(this.solution == null)
        {
            return IDA(savedNodes.ElementAt(0).Key, savedNodes);
        }
        else
        {
            return this.solution;
        }
    }

    //
    public double g(Node node)
    {
        int nbparents = 0;
        while(node.parent != null)
        {
            nbparents += 1;
            node = node.parent;
        }
        return nbparents;
    }

    //
    public double heuristique(Node node) //manhattan distance entre tout les points ordonées par la distance ou seulement le prochain point ?
    {
        // retourne la distance entre start et le desir le plus proche
        List<Case> localdesirs = desirs.ToList();
        Dictionary<Case, double> distancesDesirs = new Dictionary<Case, double>();
        for(int i=0; i<localdesirs.Count; i++)
        {
            double dist = ManhattanDist(localdesirs[i], node);
            distancesDesirs.Add(localdesirs[i], dist);
        }
        distancesDesirs = distancesDesirs.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
        return distancesDesirs.ElementAt(0).Value;
    }

    //Fonction qui retourne la distance de Manhattan entre le début et le noeud de fin
    public double ManhattanDist(Case start, Node end)
    {
        return Math.Abs(start.x - end.valeur[0]) + Math.Abs(start.y - end.valeur[1]);
    }

    //
    public List<Node> RemonterArbre()
    {
        Node solu = this.solution;
        List<Node> brancheSolution = new List<Node> { solu};
        while(solu.parent.parent != null)
        {
            solu = solu.parent;
            brancheSolution.Add(solu);
        }
        Console.WriteLine("Chemin trouvé");
        AfficherEtage(brancheSolution);
        return brancheSolution;
    }

    //
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

    // Fonction qui vérifie si - pour un des nodes de l'etage - toutes les cases ont été visité
    public Node checkSolution(List<Node> etage) 
    {
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

        //si la solution n'est pas trouvée on passe a l'étage suivant
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
        if(parent.parent == null)
        {
            if (x < 4) { output.Add( new Node(x+1, y, parent)); }
            if (x > 0) { output.Add( new Node(x-1, y, parent)); }
            if (y < 4) { output.Add( new Node(x, y+1, parent)); }
            if (y > 0) { output.Add( new Node(x, y-1, parent)); }
        }
        else if (parent.parent.valeur[0] == x - 1)
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

// Classe Node
// Comporte un compteur du nombre de cases qu'il reste à visiter, variable verifiee à la construction
// Comporte le noeud parent et une liste des noeuds enfants
class Node
{
    public List<Node> enfants;
    public Node parent;
    public int[] valeur;
    public List<Case> desirs;

    //Constructeur pour le premier Node, qui n'a pas de parent
    public Node(int x, int y, List<Case> desirs) 
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

    //Construcuteur pour le Node qui a un parent
    public Node(int x, int y, Node parent)
    {
        this.enfants = new List<Node>();
        this.parent = parent;

        this.valeur = new int[] {x,y};
        this.desirs = parent.desirs.ToList();

        int index = desirCheck();  // si le noeud créer fait partie des désirs, on le retire de la liste
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
