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
        Aspirateur aspi = new Aspirateur(4,2, false);

        /*while (true)
        {
            env.AfficherEnv(aspi);
            env.GenererObstacles(); 
            Console.WriteLine(aspi.posX.ToString()+","+aspi.posY.ToString());
            Console.ReadKey();
            aspi.capteur.ObserveEnv(env);
            bool manoirChanged = aspi.UpdateMyState();
            aspi.ChooseAnAction(manoirChanged);
            env.AfficherEnv(aspi);

        }*/
        
        // Lance les 2 fils d'execution
        Task task1 = Task.Factory.StartNew(() => env.runEnv(aspi));
        Task task2 = Task.Factory.StartNew(() => aspi.runAspi(env));

        // Attend 20 secondes avant de terminer l'experience
        Thread.Sleep(0 * 1000);
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
    public double Performance; //Score de performance du robot
    //public Aspirateur asp;
    //public Aspirateur Asp { get { return this.asp;} set { this.asp = value;} }

    public Environnement()
    {
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

                if ((random.Next(100) < 2))
                {
                    if(manoir[i, j].poussieres == false) 
                    { 
                        manoir[i, j].poussieres = true;
                    }
                }
                if ((random.Next(100) < 2) )
                {
                    if (manoir[i,j].bijoux == false)
                    {
                        manoir[i, j].bijoux = true;
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
    public int compteur_elec;  //Cout
    public bool exploration_informe; //True pour ida* et false pour bfs
    public static bool isRunning;
    List<Case> desirs = new List<Case>();
    public List<Case> Desirs { get { return desirs; } }
    public Capteur capteur = new Capteur();
    public List<Node> plan;
    public Effecteur effector = new Effecteur();

    public Aspirateur(int param_posX, int paramPosY, bool exploration_informe)
    {
        posX = param_posX;
        posY = paramPosY;
        this.exploration_informe = exploration_informe;
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
                effector.Rammasser(this,  actualCase);
            }
            else
            {
                effector.Aspirer(this, actualCase   );
            }
        }
        else // Exploration
        {
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


class Effecteur
{
    public Effecteur()
    {
        
    }
    public void Left(Aspirateur aspi)
    {
        if (aspi.posX > 0)
        {
            aspi.posX -= 1;
            aspi.compteur_elec -= 1;
        }
    }
    public void Right(Aspirateur aspi)
    {
        if (aspi.posX < 5)
        {
            aspi.posX += 1;
            aspi.compteur_elec -= 1;
        }
    }
    public void Up(Aspirateur aspi)
    {
        if (aspi.posY > 0)
        {
            aspi.posY -= 1;
            aspi.compteur_elec -= 1;
        }
    }
    public void Down(Aspirateur aspi)
    {
        if (aspi.posY < 5)
        {
            aspi.posY += 1;
            aspi.compteur_elec -= 1;
        }
    }
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
        this.solution = null;
        this.desirs = desirsparam;

        // On creer les 2 premiers etages car la fonction CreateNodes regarde la variable parent.parent
        Node A = new Node(posX, posY, desirs);  
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

    
    public Node Aetoile(Node actualNode, Dictionary<Node, double> savedNodes)
    {
        
        List<Node> child = CreateChildNodes(actualNode);
        this.solution = checkSolution(child);
        Dictionary<Node, double> temp = new Dictionary<Node, double> ();
        if(this.solution == null)
        {   
            for(int i=0; i<child.Count; i++)
            {
                double f = g(child[i]) + heuristiqueProfonde(child[i], actualNode.desirs);
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
    public double g(Node node)
    {
        //// C'est la distance parcourue jusqu'a maintenant / le nombre de desirs accomplis (nbre de désirs total - nbre de desirs du node)
        //Calcul de la distance, c'est le nombre de cases parcourues (du coup le nombre de parents)
        int nbparents = 0;
        Node temp = node;
        while(temp.parent != null)
        {
            nbparents += 1;
            temp = temp.parent;
            
        }
        int nbDesirsDone = 0;
        if(this.desirs.Count != node.desirs.Count && this.desirs.Count!=0)
        {
            nbDesirsDone = (this.desirs.Count - node.desirs.Count);
        }
        return nbparents / (nbDesirsDone + 1.0);
    }
    public double heuristique(Node node, List<Case> desirActualNode) 
    {
        // retourne la distance entre start et le desir du node le plus proche !! chaque node a une liste de desirs !!
        // comme le desirs est retiré a la creation du node il ne peux pas mettre son h à 0
        if(node.desirCheck(desirActualNode) != -1)
        {
            return 0;
        }
        else
        {
            List<Case> localdesirs = node.desirs.ToList();
            Dictionary<Case, double> distancesDesirs = new Dictionary<Case, double>();
            for(int i=0; i<localdesirs.Count; i++)
            {
                double dist = ManhattanDist(localdesirs[i], node.valeur[0], node.valeur[1]);
                distancesDesirs.Add(localdesirs[i], dist );
            }
            distancesDesirs = distancesDesirs.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            return distancesDesirs.ElementAt(0).Value;
        }
        
    }
    public double heuristiqueProfonde(Node node, List<Case> desirActualNode)
    { 

        //retrourne la distance le node et le 
        // calcule la dist avec tt les desirs et sort le plus proche
        double distancemin;
        double distaceminbis = 0;
        double distaceminbisbis = 0;
        Case selecteddesir;

        List<Case> localdesirs = node.desirs.ToList();
        Dictionary<Case, double> distancesDesirs = new Dictionary<Case, double>();
        if(node.desirCheck(desirActualNode) != -1)
        {
            distancemin = 0;
            selecteddesir = desirActualNode.ElementAt(node.desirCheck(desirActualNode));

        }
        else
        {
            for(int i=0; i<localdesirs.Count; i++)
            {
                double dist = ManhattanDist(localdesirs[i], node.valeur[0], node.valeur[1]);
                distancesDesirs.Add(localdesirs[i], dist );
            }
            distancesDesirs = distancesDesirs.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            distancemin = distancesDesirs.ElementAt(0).Value;
            selecteddesir = distancesDesirs.ElementAt(0).Key;

        }        

        // trouve le desirs le plus proche du desirs selectionné
        localdesirs.Remove(selecteddesir);
        if(localdesirs.Count != 0)
        {
            distancesDesirs = new Dictionary<Case, double>();
            for(int i=0; i<localdesirs.Count; i++)
            {
                double dist = ManhattanDist(localdesirs[i], selecteddesir.x, selecteddesir.y);
                distancesDesirs.Add(localdesirs[i], dist );
            }
            distancesDesirs = distancesDesirs.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            distaceminbis = distancesDesirs.ElementAt(0).Value;
            selecteddesir = distancesDesirs.ElementAt(0).Key;

        }
        
        localdesirs.Remove(distancesDesirs.ElementAt(0).Key);
        if(localdesirs.Count != 0)
        {
            distancesDesirs = new Dictionary<Case, double>();
            for(int i=0; i<localdesirs.Count; i++)
            {
                double dist = ManhattanDist(localdesirs[i], selecteddesir.x, selecteddesir.y);
                distancesDesirs.Add(localdesirs[i], dist );
            }
            distancesDesirs = distancesDesirs.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            distaceminbisbis = distancesDesirs.ElementAt(0).Value;
        }
        

        return distaceminbis + distancemin+distaceminbisbis;
    }
    public double ManhattanDist(Case start, int x, int y)
    {
        return Math.Abs(start.x - x) + Math.Abs(start.y - y);
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
    public List<Case> desirs;


    public Node(int x, int y, Node parent)
    {
        this.enfants = new List<Node>();
        this.parent = parent;

        this.valeur = new int[] {x,y};
        this.desirs = parent.desirs.ToList();

        int index = desirCheck(parent.desirs.ToList());  // si le noeud créer fait parti des désirs, on le retire de la liste
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

        int index = desirCheck(desirs);
        if (index!=-1)
        {
            desirs.RemoveAt(index);
        }
    }

    public int desirCheck(List<Case> desirsList) //permet de trouver le desir concerné s'il existe
    {
        int index  = - 1;
        for (int i = 0; i < desirsList.Count; i++)
        {
            if (desirsList[i].x == valeur[0] && desirsList[i].y == valeur[1])
            {
                index = i; break;
            }
        }
        return index;
    }
}
