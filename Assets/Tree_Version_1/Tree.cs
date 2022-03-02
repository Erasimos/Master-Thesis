using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree
{
    public bool grown;
    public bool stop;
    public Metamer root;
    private int age;
    public TreeDNA dna;
    private ShadowGrid shadowGrid;


    public Tree(TreeDNA dna, ShadowGrid shadowGrid)
    {
        Metamer root = new Metamer(new Vector3(0, 0, 0), new Vector3(0, 8, 0), null, null, this, shadowGrid);
        this.root = root;
        this.grown = true;
        this.stop = false;
        this.dna = dna;
        this.shadowGrid = shadowGrid;
    }

    public void Grow()
    {
        
        this.root.GatherEnergy();
        this.root.V = this.root.Q * dna.ENERGY_COEEFICENT;
        this.root.DistributeEnergy();
        dna.ENERGY_LAMBDA *= dna.APICAL_DECLINE;
        dna.ENERGY_LAMBDA = Mathf.Max(dna.ENERGY_LAMBDA, 0.49999f);
    }

    public void CalculateBranchDiameters()
    {
        this.root.CalculateBranchDiameter();
    }
    
    public class Metamer
    {

        // PARAMETERS
        

        private Tree tree;
        public Metamer main;
        public Metamer lateral;

        // ENERGY 
        public float Q;
        public float V;
        public float stored_V;

        public Vector3 bottom;
        public Vector3 top;
        public Vector3 direction;
        public float diameter;

        public Metamer(Vector3 bottom, Vector3 top, Metamer main, Metamer lateral, Tree tree, ShadowGrid shadowGrid)
        {
            this.bottom = bottom;
            this.top = top;
            this.main = main;
            this.lateral = lateral;
            this.direction = Vector3.Normalize(top - bottom);
            this.tree = tree;
            shadowGrid.updateGrid(top);
        }

        public float GatherEnergy()
        {
            // ADD SHADOW PROPOGATION
            float Energy = 0;
            float shadowValue = tree.shadowGrid.getShadowValuePos(this.top);

            if (this.main != null) Energy += this.main.GatherEnergy();
            else Energy += tree.dna.LEAF_ENERGY;
            if (this.lateral != null) Energy += this.lateral.GatherEnergy();
            else Energy += tree.dna.LEAF_ENERGY;

            Energy *= (1 - shadowValue);

            this.Q = Energy;
            return this.Q;

            
            
        }

        public void Grow(float Energy, bool isMain)
        {
            // ADD RANDOMNESS
            int segments = Mathf.FloorToInt(Energy);
            float segment_length = (Energy / segments) * 2.1f;
            Metamer currentMetamer = this;

            Vector3 previoiusTop = this.top;

            for (int i = 0; i < segments; i++)
            {


                if (isMain)
                {
                    // Main Metamer
                    float rotationAngle = MathHelper.GetRandom(0, 360);
                    float branchAngle = MathHelper.GetRandom(0, tree.dna.BRANCHING_ANGLE_MAIN);
                    Vector3 perpendicularDirection = MathHelper.GetPerpendicularVector(this.direction);
                    Vector3 mainDirection = Quaternion.AngleAxis(branchAngle, perpendicularDirection) * this.direction;
                    mainDirection = Quaternion.AngleAxis(rotationAngle, this.direction) * mainDirection;
                    Vector3 newTop = previoiusTop + (mainDirection * segment_length);
                    Metamer newMain = new Metamer(previoiusTop, newTop, null, null, this.tree, this.tree.shadowGrid);
                    currentMetamer.main = newMain;
                    currentMetamer = newMain;

                    previoiusTop = newTop;
                }
                else
                {
                    segment_length *= 0.55f; 
                    // Lateral Metamer
                    float rotationAngle = MathHelper.GetRandom(0, 360);
                    float branchAngle = MathHelper.GetRandom(tree.dna.BRANCHING_ANGLE_LATERAL - 5f, tree.dna.BRANCHING_ANGLE_LATERAL + 5f);
                    Vector3 perpendicularDirection = MathHelper.GetPerpendicularVector(this.direction);
                    Vector3 lateralDirection = Quaternion.AngleAxis(branchAngle, perpendicularDirection) * this.direction;
                    lateralDirection = Quaternion.AngleAxis(rotationAngle, this.direction) * lateralDirection;
                    Vector3 newTop = previoiusTop + (lateralDirection * segment_length);
                    Metamer newLateral = new Metamer(previoiusTop, newTop, null, null, this.tree, this.tree.shadowGrid);
                    currentMetamer.lateral = newLateral;
                    currentMetamer = newLateral;

                    previoiusTop = newTop;
                }
            }
        }

        public void DistributeEnergy()
        {
            float main_Q = (this.main != null) ? this.main.Q : tree.dna.LEAF_ENERGY;
            float lateral_Q = (this.lateral != null) ? this.lateral.Q : tree.dna.LEAF_ENERGY;
            this.V += this.stored_V;     
            float nom = this.V * tree.dna.ENERGY_LAMBDA * main_Q;
            float denom = tree.dna.ENERGY_LAMBDA * main_Q + (1 - tree.dna.ENERGY_LAMBDA) * lateral_Q;
            float ENERGY_MAIN = nom / denom;
            float ENERGY_LATERAL = this.V * (1 - tree.dna.ENERGY_LAMBDA) * lateral_Q / (tree.dna.ENERGY_LAMBDA * main_Q + (1 - tree.dna.ENERGY_LAMBDA) * lateral_Q);

            if (this.main == null)
            {
                if (ENERGY_MAIN < 1) this.stored_V += ENERGY_MAIN;
                else this.Grow(ENERGY_MAIN, true);

            }
            else
            {
                this.main.V = ENERGY_MAIN;
                this.main.DistributeEnergy();
            }
            if (this.lateral == null)
            {
                if (ENERGY_LATERAL < 1) this.stored_V += ENERGY_LATERAL;
                else this.Grow(ENERGY_LATERAL, false);
            }
            else
            {
                this.lateral.V = ENERGY_LATERAL;
                this.lateral.DistributeEnergy();
            }
        }

        public float CalculateBranchDiameter()
        {
            if (this.main == null & this.lateral == null)
            {
                this.diameter = tree.dna.MIN_DIAMETER;
                return tree.dna.MIN_DIAMETER;
            }
            else
            {
                float d_squared = 0;

                if (this.main != null) d_squared += Mathf.Pow(this.main.CalculateBranchDiameter(), tree.dna.BRANCH_DIAMTER_n);
                else d_squared += Mathf.Pow(tree.dna.MIN_DIAMETER, tree.dna.BRANCH_DIAMTER_n);
                if (this.lateral != null) d_squared += Mathf.Pow(this.lateral.CalculateBranchDiameter(), tree.dna.BRANCH_DIAMTER_n);
                else d_squared += Mathf.Pow(tree.dna.MIN_DIAMETER, tree.dna.BRANCH_DIAMTER_n);
                this.diameter = Mathf.Pow(d_squared, 1.0f / tree.dna.BRANCH_DIAMTER_n);
                return this.diameter;
            }
        }
    }
}
