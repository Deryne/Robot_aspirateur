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
        Environnement env = new Environnement();
        env.GenererObstacles();
        Aspirateur aspi = new Aspirateur(4,2, true);
        
        // Lance les 2 fils d'execution
        Task task1 = Task.Factory.StartNew(() => env.runEnv(aspi));
        Task task2 = Task.Factory.StartNew(() => aspi.runAspi(env));

        // Attend 20 secondes avant de terminer l'experience
        Thread.Sleep(0 * 1000);
        Console.ReadKey();
    }
}

//Classe Caase
//Sert à contenir des bijoux et/ou de la poussière
class Case
{
    //Proprités de la pièce ici appelée "case"
    public bool bijoux; // Il peut y avoir un bijoux dedans
    public bool poussieres; //Il peut y avoir de la poussière dedans (aussi)

    public int x; // Permet de calculer la distance, sera utile pour A*
    public int y;

    //Définition des coordonnées dans un constructeur
    public Case(int xparam, int yparam)
    {
        bijoux = false;
        poussieres = false;

        this.x = xparam;
        this.y = yparam;
    }
}

//Classe Environnement
//Sert à créer un environnement de cases pour l'aspirateur
class Environnement
{
    //L'environement est composé de 25 cases avec des propriétés      
    public Case[,] manoir = new Case[5, 5];
    public double performance; //Score de performance du robot

    //Constructeur
    public Environnement()
    {
        //On crée d'abord un manoir vide
        CreerEnvVide();
    }

    //Fonction qui initialise le manoir
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

    //Fonction qui génère les bijoux et les poussières
    public void GenererObstacles()
    {
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                Random random = new Random();

                if ((random.Next(100) < 2))//Probabilité en dessous de 2%
                {
                    if(manoir[i, j].poussieres == false) 
                    { 
                        manoir[i, j].poussieres = true;
                    }
                }
                if ((random.Next(100) < 2))//Probabilité en dessous de 2%
                {
                    if (manoir[i,j].bijoux == false)
                    {
                        manoir[i, j].bijoux = true;
                    }   
                }
            }
        }
        
    }

    //Fonction qui affiche l'environnement en temps réel
    public void AfficherEnv(Aspirateur asp)
    {
        Console.WriteLine("Affichage du terrain");
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {   
                if(asp.plan != null && asp.plan.Any(x => x.valeur[0] == j && x.valeur[1] == i) == true)
                {
                    Console.BackgroundColor = ConsoleColor.Magenta;
                }
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

    public double MesurePerformance(Aspirateur aspi)
    { 
        // a tte les iterations l'env va check le nombre de cases occupees pondéré par le compteur+nbbijouxaspires
        int nbCasesVides = 0;
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                if (manoir[i,j].bijoux == false && manoir[i,j].poussieres == false)
                {
                    nbCasesVides++;
                }
            }
        }
        //aspi.effector.NbBijouxRamasses = 0;
        //aspi.effector.NbPoussieresAspirees = 0;
        //aspi.effector.NbBijouxAspires = 0;
        return 5;
    }

    //Thread de l'environnement
    public void runEnv(Aspirateur asp)
    {
        while (true)
        {
            this.GenererObstacles();
            performance = MesurePerformance(asp);
            Thread.Sleep(5000);
        }
    }
}

//Classe Aspirateur
//Sert à créer le robot aspirateur
class Aspirateur
{
    public int posX;//Position en X
    public int posY;//Position en Y
    public int compteur_elec = 0; //Cout d'electricite
    public bool exploration_informe; //True pour ida* et false pour bfs
    public static bool isRunning;
    List<Case> intentions = new List<Case>();//Liste vide des intentions de l'aspirateur
    public List<Case> Intentions { get { return intentions; } }
    public Capteur capteur = new Capteur();//Création d'un capteur
    public List<Node> plan;//Création d'un plan d'exécution
    public Effecteur effector = new Effecteur();//Création d'un effecteur
    public int Compteur_elec { get { return this.compteur_elec; } set{this.compteur_elec = value;}}
    //Constructeur
    public Aspirateur(int param_posX, int paramPosY, bool exploration_informe)
    {
        posX = param_posX;
        posY = paramPosY;
        this.exploration_informe = exploration_informe;
    }

    //Thread de l'aspirateur
    /*
     * la performance consiste en le nb de cases hors du plan, apprendre la meilleur freq d'exploration signifie que l'exploration prend du temps (pourquoi l'optimiser sinon)
     * Donc pour que ca soit realiste il faut que le temps d'exploration soit superieur au temps de pause
    */
    public void runAspi(Environnement env)
    {
        int performanceMini;
        bool manoirChanged = true;
        while (true) { 
            this.capteur.ObserveEnv(env);
            manoirChanged = this.UpdateMyState();
            env.AfficherEnv(this);
            this.ChooseAnAction(manoirChanged, true);
            Thread.Sleep(1000);
        }
    }

    // Mets à jour les intentions
    public bool UpdateMyState() 
    {
        bool manoirChanged = false;
        List<Case> intentionsTemp = intentions.ToList();
        intentions = new List<Case>();

        foreach(Case i in capteur.beliefs)
        {
            if(i.bijoux == true || i.poussieres == true)
            {
                intentions.Add(i);
            }
        }   
        foreach(Case c in intentions)
        {
            if (intentionsTemp.Contains(c) == false)
            {
                manoirChanged = true;
            }
        }
        return manoirChanged;
    }

    //Choisis la première intention à réaliser
    public void ChooseAnAction(bool manoirChanged, bool exploration)
    {
        // Verifie si le robot se trouve sur une case avec une intention
        int index  = - 1;
        for (int i = 0; i < intentions.Count; i++)
        {
            if (intentions[i].x == posX && intentions[i].y == posY)
            {
                index = i; break;
            }
        }
        if(index != -1)
        {
            Case actualCase = intentions[index];
            if (intentions[index].bijoux == true)
            {
                effector.Rammasser(this,  actualCase);
            }
            else
            {
                effector.Aspirer(this, actualCase   );
            }
            intentions.RemoveAt(index);
        }
        
        if(manoirChanged == true && exploration == true)
        {   
            var start = DateTime.Now;
            Graph tree = new Graph(exploration_informe, posX, posY, intentions);  // génère le graph en BFS et trouve la solution
            plan = tree.RemonterArbre();  // remonte la solution pour trouver le prochain mouvement
            Console.WriteLine("                                                                 "+(DateTime.Now-start).ToString());
        }
        if(plan !=  null && plan.Count != 0)
        {
            Node nexMoove = plan[plan.Count-1];
            plan.Remove(nexMoove);

            if(posX == nexMoove.valeur[0]) //ça veut dire que le mouvement est dans la dimension y
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

//Classe Capteur
//Sert à créer une copie de l'environnement
class Capteur
{
    public Case[,] beliefs = new Case[5, 5];//Création d'une copie vide de l'environnement
    public double performance;

    //Constrcteur vide
    public Capteur() { }

    //Constructeur
    public Capteur(Environnement env)
    {
        ObserveEnv(env);
    }

    //Remplissage de la copie
    public void ObserveEnv(Environnement env)
    {
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                beliefs[i, j] = env.manoir[i, j];
            }
        }
    }
    
}

//Classe Effecteur
//Sert à faire agir le robot
class Effecteur
{
    public int NbPoussieresAspirees = 0;
    public int NbBijouxAspires=0;
    public int NbBijouxRamasses=0;
    //Constructeur vide
    public Effecteur(){}

    //Aller à gauche
    public void Left(Aspirateur aspi)
    {
        if (aspi.posX > 0)
        {
            aspi.posX -= 1;
            aspi.Compteur_elec += 1;
        }
    }

    //Aller à droite
    public void Right(Aspirateur aspi)
    {
        if (aspi.posX < 4)
        {
            aspi.posX += 1;
            aspi.Compteur_elec += 1;
        }
    }

    //Aller en haut
    public void Up(Aspirateur aspi)
    {
        if (aspi.posY > 0)
        {
            aspi.posY -= 1;
            aspi.Compteur_elec += 1;
        }
    }

    //Aller en bas
    public void Down(Aspirateur aspi)
    {
        if (aspi.posY < 4)
        {
            aspi.posY += 1;
            aspi.Compteur_elec += 1;
        }
    }

    //Ramasser un bijoux
    public void Rammasser(Aspirateur aspi, Case param_case)
    {
        param_case.bijoux = false;
        aspi.Compteur_elec += 1;
        Console.WriteLine("-----------------------------------------Ramasser");
        NbBijouxRamasses++;
    }

    //Aspirer une poussière
    public void Aspirer(Aspirateur aspi, Case param_case)
    {
        param_case.poussieres = false;
        if (param_case.bijoux)
        {
            param_case.bijoux = false;
            NbBijouxAspires++;
        }
        else
        {
            NbPoussieresAspirees++;
        }
        aspi.compteur_elec += 1;
        Console.WriteLine("-----------------------------------------Aspirer");
    }

}

/// <summary>
///  Cette classe permet d'appliquer plusieur aglorithmes d'exploration à notre champs de recherche
///  Au lieu de créer un graph puis de le parcourir on l'explore en même temps qu'on le crée
/// </summary>
class Graph
{
    //solution cherchée : manoir propre --> le robot passe sur toutes les cases 
    public List<Node> graph; //liste de nodes reliés par leur attributs parent et enfants
    public List<Case> intentions;  // liste des cases à visiter
    public Node solution;   // Noeud solution trouvé, il suffit ensuite de remonter le chemin grace aux attributs 
   
    //Constructeur
    public Graph(bool exploration_informe, int posX, int posY, List<Case> intentionsparam)
    {
        this.solution = null;
        this.intentions = intentionsparam;

        // On creer les 2 premiers etages car la fonction CreateNodes regarde la variable parent.parent
        Node A = new Node(posX, posY, intentions);  
        if(exploration_informe == false)
        {
            if (posX > 0) { A.enfants.Add(new Node(posX-1, posY, A)); }
            if (posX < 4) { A.enfants.Add(new Node(posX+1, posY, A)); }
            if (posY > 0) { A.enfants.Add(new Node(posX, posY-1, A)); }
            if (posY < 4) { A.enfants.Add(new Node(posX, posY+1, A)); }
            this.solution = checkSolution(A.enfants.ToList());
            if(this.solution == null)
            {
                this.solution = BFS(A.enfants.ToList());
            }
        }
        else
        {             
            Dictionary<Node, double> temp = new Dictionary<Node, double> ();
            temp.Add(A, 10000);
            this.solution = Aetoile(A,temp);
        }
    }

    //Fonction pour A*
    public Node Aetoile(Node actualNode, Dictionary<Node, double> savedNodes)
    {
        
        List<Node> child = CreateChildNodes(actualNode);
        this.solution = checkSolution(child);
        Dictionary<Node, double> temp = new Dictionary<Node, double> ();
        if(this.solution == null)
        {   
            for(int i=0; i<child.Count; i++)
            {
                double f = g(child[i]) + heuristiqueProfonde(child[i], actualNode.intentions);
                savedNodes.Add(child[i], f);
                temp.Add(child[i], f);
            }
            savedNodes.Remove(actualNode);
            /*
            Console.WriteLine(actualNode.valeur[0].ToString() + ";" + actualNode.valeur[1].ToString());
            //Console.WriteLine("contains"+savedNodes.Count.ToString());
            
            foreach(Node a in savedNodes.Keys){
                Node b = a;
                while(b.parent != null)
                {
                    Console.Write(b.valeur[0].ToString() + ";" + b.valeur[1].ToString() + " ");
                    b = b.parent;
                }

                Console.Write(savedNodes[a]);
                Console.WriteLine("---------");
            }
            Console.WriteLine("*******");*/

            savedNodes = savedNodes.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            temp = temp.OrderBy(x => x.Value).ToDictionary(x =>x.Key, temp => temp.Value);  
        }
        if(this.solution == null)
        {
            // on expanse le noeud ayant le plus bas score et en priorisant les enfants du noeud actuel
            if(savedNodes.Count > 1000)
            {
                savedNodes = savedNodes.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

                savedNodes = savedNodes.Take(5).ToDictionary(x=>x.Key, x=> x.Value);
            }
            Node next;
            if (temp.ElementAt(0).Value == savedNodes.ElementAt(0).Value)
            {
                next = temp.ElementAt(0).Key;
            }
            else
            {
                next = savedNodes.ElementAt(0).Key;
            }
            return Aetoile(next, savedNodes);

        }
        else
        {
            return this.solution;
        }
    }

    //Fonction g
    public double g(Node node)
    {
        // C'est la distance parcourue jusqu'a maintenant / le nombre d'intentions accomplies (nbre de désirs total - nbre d'intentions du node)
        // Calcul de la distance, c'est le nombre de cases parcourues (du coup le nombre de parents)
        int nbparents = 0;
        Node temp = node;
        while(temp.parent != null)
        {
            nbparents += 1;
            temp = temp.parent;
            
        }
        int nbIntentionsDone = 0;
        if(this.intentions.Count != node.intentions.Count && this.intentions.Count!=0)
        {
            nbIntentionsDone = (this.intentions.Count - node.intentions.Count);
        }
        return nbparents / (nbIntentionsDone + 1.0);
    }

    //Fonction heuristique
    public double heuristique(Node node, List<Case> intentionActualNode) 
    {
        // retourne la distance entre start et le desir du node le plus proche !! chaque node a une liste des intentions !!
        // comme l'intention est retiré a la creation du node il ne peux pas mettre son h à 0
        if(node.desirCheck(intentionActualNode) != -1)
        {
            return 0;
        }
        else
        {
            List<Case> localintentions = node.intentions.ToList();
            Dictionary<Case, double> distancesIntentions = new Dictionary<Case, double>();
            for(int i=0; i<localintentions.Count; i++)
            {
                double dist = ManhattanDist(localintentions[i], node.valeur[0], node.valeur[1]);
                distancesIntentions.Add(localintentions[i], dist );
            }
            distancesIntentions = distancesIntentions.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            return distancesIntentions.ElementAt(0).Value;
        }
        
    }

    public double ManhattanDist(Case start, int x, int y)
    {
        return Math.Abs(start.x - x) + Math.Abs(start.y - y);
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
        Console.WriteLine("Chemin trouve");
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

    //
    public Node checkSolution(List<Node> etage) 
    {
        // Vérifie si - pour un des nodes de l'etage - toutes les cases ont été visité
        for (int i =0; i<etage.Count; i++)
        {
            if(etage[i].intentions.Count == 0)
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

// Comporte un compteur du nombre de cases qu'il reste à visiter, variable verifiee à la construction
// Comporte le noeud parent et une liste des noeuds enfants
class Node
{
    public List<Node> enfants;
    public Node parent;
    public int[] valeur;
    public List<Case> intentions;


    public Node(int x, int y, Node parent)
    {
        this.enfants = new List<Node>();
        this.parent = parent;

        this.valeur = new int[] {x,y};
        this.intentions = parent.intentions.ToList();

        int index = desirCheck(parent.intentions.ToList());  // si le noeud créer fait parti des désirs, on le retire de la liste
        if (index!=-1)
        {
            intentions.RemoveAt(index);
        }
    }

    public Node(int x, int y, List<Case> intentions) //constructeur pour le premier Node, qui n'a pas de parent
    {
        this.enfants = new List<Node>();
        this.parent = null;
        
        this.valeur = new int[] {x,y};
        this.intentions = intentions;

        int index = desirCheck(intentions);
        if (index!=-1)
        {
            intentions.RemoveAt(index);
        }
    }

    public int desirCheck(List<Case> intentionsList) //permet de trouver le desir concerné s'il existe
    {
        int index  = - 1;
        for (int i = 0; i < intentionsList.Count; i++)
        {
            if (intentionsList[i].x == valeur[0] && intentionsList[i].y == valeur[1])
            {
                index = i; break;
            }
        }
        return index;
    }
}
