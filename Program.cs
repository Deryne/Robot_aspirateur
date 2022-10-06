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
        string reponse = "";
        bool informee = false;
        while(reponse != "false" && reponse != "true")
        {
            Console.WriteLine("Choisissez une exploration (false = BFS et true = A*)");
            reponse = Console.ReadLine();
        }
        if(reponse == "true")
        {
            Console.WriteLine("Exploration informée A*");
            informee = true;
        }
        else{ Console.WriteLine("Exploration non informée BFS"); }


        Environnement env = new Environnement();
        env.GenererObstacles();
        Aspirateur aspi = new Aspirateur(4,2, true);


        // Lance les 2 fils d'execution
        Task task1 = Task.Factory.StartNew(() => env.runEnv(aspi));
        Task task2 = Task.Factory.StartNew(() => aspi.runAspi(env));
          
        Console.ReadKey();
    }
}

//Classe Case
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
        Console.WriteLine("                                                                        Génération d'obstacles");
        Random random = new Random();

        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                
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
        Console.WriteLine(":::::::::::::::::::::::::::::::::::::::::::::::::");
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
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.Write(" B ");
                        break;
                    case (true, true):
                         Console.ForegroundColor = ConsoleColor.Yellow;
                         Console.Write(" X ");
                         break;
                    case (false, true):
                        Console.ForegroundColor = ConsoleColor.Green;
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
        // a toute les iterations l'env va check le nombre de cases occupees pondéré par le compteur electrique+nbbijouxaspires
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
        return (nbCasesVides / aspi.compteur_elec) - aspi.effector.NbBijouxAspires;
    }

    //Thread de l'environnement
    public void runEnv(Aspirateur asp)
    {
        while (true)
        {
            this.GenererObstacles();
            Thread.Sleep(4000);
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
    List<Case> casesCibles = new List<Case>();//Liste vide des cases à  visiter pour l'aspirateur
    public Capteur capteur = new Capteur();//Création d'un capteur
    public List<Node> plan;//Création d'un plan d'exécution
    public Effecteur effector = new Effecteur();//Création d'un effecteur

    //Constructeur
    public Aspirateur(int param_posX, int paramPosY, bool exploration_informe)
    {
        posX = param_posX;
        posY = paramPosY;
        this.exploration_informe = exploration_informe;
    }

    //Thread de l'aspirateur
    public void runAspi(Environnement env)
    {
        bool manoirChanged = true; // Le robot va faire tourner l'exploration seulement si le manoir a changé
        while (true) { 
            this.capteur.ObserveEnv(env);
            manoirChanged = this.UpdateMyState();
            env.AfficherEnv(this);
            this.ChooseAnAction(manoirChanged, true);
            Thread.Sleep(1000);
        }
    }

    // Mets à jour les cases à visiter
    public bool UpdateMyState() 
    {
        bool manoirChanged = false;
        List<Case> casesCiblesTemp = casesCibles.ToList();
        casesCibles = new List<Case>();

        foreach(Case i in capteur.beliefs)
        {
            if(i.bijoux == true || i.poussieres == true)
            {
                casesCibles.Add(i);
            }
        }   
        foreach(Case c in casesCibles)
        {
            if (casesCiblesTemp.Contains(c) == false)
            {
                manoirChanged = true;
            }
        }
        return manoirChanged;
    }

    public void ChooseAnAction(bool manoirChanged, bool exploration)
    {
        // Verifie si le robot se trouve sur une case cible
        int index  = - 1;
        for (int i = 0; i < casesCibles.Count; i++)
        {
            if (casesCibles[i].x == posX && casesCibles[i].y == posY)
            {
                index = i; break;
            }
        }
        if(index != -1)
        {
            Case actualCase = casesCibles[index];
            if (casesCibles[index].bijoux == true)
            {
                effector.Rammasser(this,  actualCase);
            }
            else
            {
                effector.Aspirer(this, actualCase   );
            }
            casesCibles.RemoveAt(index);
        }
        
        if(manoirChanged == true && exploration == true)
        {   
            var start = DateTime.Now;
            Graph tree = new Graph(exploration_informe, posX, posY, casesCibles);  // génère le graph en BFS et trouve la solution
            plan = tree.RemonterArbre();  // remonte la solution pour trouver le prochain mouvement
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
    public int NbBijouxAspires=0;
    //Constructeur vide
    public Effecteur(){}

    //Aller à gauche
    public void Left(Aspirateur aspi)
    {
        if (aspi.posX > 0)
        {
            aspi.posX -= 1;
            aspi.compteur_elec += 1;
        }
    }

    //Aller à droite
    public void Right(Aspirateur aspi)
    {
        if (aspi.posX < 4)
        {
            aspi.posX += 1;
            aspi.compteur_elec += 1;
        }
    }

    //Aller en haut
    public void Up(Aspirateur aspi)
    {
        if (aspi.posY > 0)
        {
            aspi.posY -= 1;
            aspi.compteur_elec += 1;
        }
    }

    //Aller en bas
    public void Down(Aspirateur aspi)
    {
        if (aspi.posY < 4)
        {
            aspi.posY += 1;
            aspi.compteur_elec += 1;
        }
    }

    //Ramasser un bijoux
    public void Rammasser(Aspirateur aspi, Case param_case)
    {
        param_case.bijoux = false;
        aspi.compteur_elec += 1;
        Console.WriteLine("-----------------------------------------Ramasser");
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
    public List<Case> casesCibles;  // liste des cases à visiter
    public Node solution;   // Noeud solution trouvé, il suffit ensuite de remonter le chemin grace aux attributs 
   
    //Constructeur
    public Graph(bool exploration_informe, int posX, int posY, List<Case> casesCiblesparam)
    {
        this.solution = null;
        this.casesCibles = casesCiblesparam;

        Node A = new Node(posX, posY, casesCibles); 
        
        if(exploration_informe == false)
        {
            // On creer les 2 premiers etages car la fonction CreateNodes regarde la variable parent.parent
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

    //Fonction reccursive pour A*
    public Node Aetoile(Node actualNode, Dictionary<Node, double> savedNodes)
    {
        List<Node> child = CreateChildNodes(actualNode);   //creation des enfants du noeud d'entree
        this.solution = checkSolution(child);              //vérification de la solution atteinte
        //dictionnaire pour les enfants créés précédemment, on ne les ajoute pas directement au dictionnaire principal car on veut pouvoir les séléctionner en priorité  
        Dictionary<Node, double> temp = new Dictionary<Node, double> (); 
        if(this.solution == null)
        {   
            // pour chaque enfant on calcule sa distance à la case cible la plus proche
            for(int i=0; i<child.Count; i++)
            {
                double f = g(child[i]) + heuristique(child[i], actualNode.casesCibles);
                savedNodes.Add(child[i], f);  // les enfants sont aussi ajoutés au dicitonnaire principal
                temp.Add(child[i], f);
            }
            savedNodes.Remove(actualNode);    

            // on trie les dicitonnaire par cout total
            savedNodes = savedNodes.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            temp = temp.OrderBy(x => x.Value).ToDictionary(x =>x.Key, temp => temp.Value);  

            if(savedNodes.Count > 1000) {savedNodes = savedNodes.Take(10).ToDictionary(x=>x.Key, x=> x.Value); } // au cas où les limites de mémoires dépassent                   
            
            //Si un des nouveau enfant fait parti des noeuds ayant le cout minimum, on le choisit, sinon on choisit le noeud le plus ancien (et donc le plus haut dans l'arbre) ayant un cout minimal 
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
        // C'est la distance parcourue jusqu'a maintenant / le nombre de cases cibles déjà visitées
        // Calcul de la distance, c'est le nombre de cases parcourues (du coup le nombre de parents)
        int nbparents = 0;
        Node temp = node;
        while(temp.parent != null)
        {
            nbparents += 1;
            temp = temp.parent;
            
        }
        int nbCasesCibleVisitees = 0;
        if(this.casesCibles.Count != node.casesCibles.Count && this.casesCibles.Count!=0)
        {
            nbCasesCibleVisitees = (this.casesCibles.Count - node.casesCibles.Count);
        }
        return nbparents / (nbCasesCibleVisitees + 1.0) ;
    }

    //Fonction heuristique
    // retourne la distance entre node et le desir du node le plus proche !! chaque node a une liste des cases cibles qu'il lui reste à visiter !!
    public double heuristique(Node node, List<Case> intentionActualNode) 
    {
        if(node.desirCheck(intentionActualNode) != -1)
        {
            return 0;
        }
        else
        {
            List<Case> localCasesCibles = node.casesCibles.ToList();
            Dictionary<Case, double> distancesCibles = new Dictionary<Case, double>();
            for(int i=0; i<localCasesCibles.Count; i++)
            {
                double dist = ManhattanDist(localCasesCibles[i], node.valeur[0], node.valeur[1]);
                distancesCibles.Add(localCasesCibles[i], dist );
            }
            distancesCibles = distancesCibles.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            return distancesCibles.ElementAt(0).Value;
        }
        
    }

    public double ManhattanDist(Case start, int x, int y)
    {
        return Math.Abs(start.x - x) + Math.Abs(start.y - y);
    }    

    // Remonte l'arbre a parti du noeud solution jusqu'au noeud racine
    public List<Node> RemonterArbre()
    {
        Node solu = this.solution;
        List<Node> brancheSolution = new List<Node> { solu};
        while(solu.parent.parent != null)
        {
            solu = solu.parent;
            brancheSolution.Add(solu);
        }
        Console.WriteLine("                                                                        Chemin trouvé : ");
        AfficherEtage(brancheSolution);
        return brancheSolution;
    }

    //
    public void AfficherEtage(List<Node> etage)
    {
        string str = "                                                                      ";
        foreach(Node n in etage)
        {
             str += "  " + n.valeur[0].ToString() + "," + n.valeur[1].ToString();
        }
        Console.WriteLine(str);
        Console.WriteLine();
    }

    // Vérifie si - pour un des nodes de l'etage - toutes les cases ont été visité
    public Node checkSolution(List<Node> etage) 
    {
        for (int i =0; i<etage.Count; i++)
        {
            if(etage[i].casesCibles.Count == 0)
            {
                return etage[i];
            }
        }
        return null;
    }


    //  Cette fonction récursive génère l'arbre étage par étage jusqu'a trouver la solution
    public Node BFS(List<Node> etagesup)
    {
        List<Node> etageActuel = new List<Node>();

        // pour chaque noeud de l'etage superieur, on lui ajoute 4 noeuds enfants : droite, gauche, haut, bas (sauf limite de l'env atteintes)
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
        // Créer 4 Nodes enfants, prend en compte les sorties d'espace de recherche
        int x = parent.valeur[0];
        int y = parent.valeur[1];
        List<Node> output = new List<Node>();
        
        if (x < 4) { output.Add( new Node(x+1, y, parent)); }
        if (x > 0) { output.Add( new Node(x-1, y, parent)); }
        if (y < 4) { output.Add( new Node(x, y+1, parent)); }
        if (y > 0) { output.Add( new Node(x, y-1, parent)); }
        
        
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
    public List<Case> casesCibles;


    public Node(int x, int y, Node parent)
    {
        this.enfants = new List<Node>();
        this.parent = parent;

        this.valeur = new int[] {x,y};
        this.casesCibles = parent.casesCibles.ToList();

        int index = desirCheck(parent.casesCibles.ToList());  // si le noeud créer fait parti des cases cibles, on le retire de la liste
        if (index!=-1)
        {
            casesCibles.RemoveAt(index);
        }
    }

    public Node(int x, int y, List<Case> casesCibles) //constructeur pour le premier Node, qui n'a pas de parent
    {
        this.enfants = new List<Node>();
        this.parent = null;
        
        this.valeur = new int[] {x,y};
        this.casesCibles = casesCibles;

        int index = desirCheck(casesCibles);
        if (index!=-1)
        {
            casesCibles.RemoveAt(index);
        }
    }

    public int desirCheck(List<Case> casesCiblesList) //permet de trouver le desir concerné s'il existe
    {
        int index  = - 1;
        for (int i = 0; i < casesCiblesList.Count; i++)
        {
            if (casesCiblesList[i].x == valeur[0] && casesCiblesList[i].y == valeur[1])
            {
                index = i; break;
            }
        }
        return index;
    }
}

