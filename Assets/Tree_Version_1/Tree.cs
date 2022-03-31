using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree
{
    public bool grown = false;
    public bool stop;
    public Branch root;
    public TreeDNA dna;
    private ShadowGrid shadowGrid;
    public Vector3 position;
    private int age;
    public List<Branch> newBranches;

    public Tree(TreeDNA dna, ShadowGrid shadowGrid, Vector3 pos)
    {
        
        stop = false;
        this.dna = dna;
        this.shadowGrid = shadowGrid;
        age = 0;
        Branch root = new Branch(pos, pos + new Vector3(0, dna.SHOOT_LENGTH, 0), this, shadowGrid, 1);
        this.root = root;
        newBranches = new List<Branch>();
        newBranches.Add(root);
        grown = true;
    }

    public void Grow()
    {
        age += 1;
        //dna.GRAVITROPISM_WIEGHT *= dna.GRAVITROPISM_DECLINE;
        dna.GRAVITROPISM -= new Vector3(0, dna.GRAVITROPISM_DECLINE, 0);
        if (age > dna.MAX_AGE) stop = true;
        root.GatherEnergy();
        root.SheddBranches();
        root.V = root.Q * dna.ENERGY_COEEFICENT;
        root.DistributeEnergy();
        CalculateBranchDiameters();
        
        grown = true;
    }

    public void CalculateBranchDiameters()
    {
        this.root.CalculateBranchDiameter();
    }
    
    public class Branch
    {
        public Tree tree;
        public Branch main;
        public Branch lateral;
        public int depth;

        public bool growing;

        // ENERGY 
        public float Q;
        public float V;
        public float stored_V;

        public Vector3 bottom;
        public Vector3 top;
        public Vector3 direction;
        public float diameter;

        public Branch(Vector3 bottom, Vector3 top, Tree tree, ShadowGrid shadowGrid, int depth)
        {
            this.bottom = bottom;
            this.top = top;
            direction = Vector3.Normalize(top - bottom);
            this.tree = tree;
            shadowGrid.updateGrid(top);
            this.depth = depth;
            growing = true;
        }

        public float GatherEnergy()
        {
            float Energy = 0;

            // Gather energy from existing children branches
            if (main != null) Energy += main.GatherEnergy();
            else Energy += tree.shadowGrid.getLightExposure(top);
            if (lateral != null) Energy += lateral.GatherEnergy();
            else Energy += tree.shadowGrid.getLightExposure(top);

            Q = Energy;
            return Q;
        }

        public Vector3 findOptimalGrowthDirection(float angle)
        {
            Vector3 optimalDirection = new Vector3(0, 0, 0);
            float minShadowvalue = Mathf.Infinity;

            for (int i = 0; i < tree.dna.DIRECTION_SAMPLES; i++)
            {
                Vector3 sampleDirection = MathHelper.SamplePerceptionSphere(direction, angle) + tree.dna.GRAVITROPISM * tree.dna.GRAVITROPISM_WIEGHT + direction * tree.dna.SELFTROPISM_WEIGHT;
                float sampleStep = tree.dna.SHOOT_LENGTH / tree.dna.SHADOW_SAMPLES;
                float shadowValue = 0;

                for (int j = 0; j < tree.dna.SHADOW_SAMPLES; j++)
                {
                    Vector3 samplePos = top + sampleDirection * sampleStep * j;
                    shadowValue += tree.shadowGrid.getShadowValuePos(samplePos);
                }

                if (shadowValue < minShadowvalue)
                {
                    minShadowvalue = shadowValue;
                    optimalDirection = sampleDirection;
                }
            }
            return optimalDirection.normalized;
        }

        public void Grow(float Energy, bool isMain)
        {
            float angle = (isMain) ? tree.dna.PERCEPTION_ANGLE / 3.0f : tree.dna.PERCEPTION_ANGLE;
            Branch currentBranch = this;

            int numberOfNewBranches = Mathf.FloorToInt(Energy);

            if (numberOfNewBranches == 0) stored_V += Energy;

            for (int i = 0; i < numberOfNewBranches; i ++)
            {
                Vector3 branchDirection = findOptimalGrowthDirection(angle);
                Vector3 newBranchBottom = currentBranch.top;
                Vector3 newBranchTop = newBranchBottom + branchDirection * tree.dna.SHOOT_LENGTH * Mathf.Pow(0.95f, depth);
                Branch newBranch = new Branch(newBranchBottom, newBranchTop, tree, tree.shadowGrid, depth += 1);

                if (isMain) currentBranch.main = newBranch;
                else currentBranch.lateral = newBranch;
                    
                currentBranch = newBranch;
            }
        }

        public void DistributeEnergy()
        {
            float main_Q = (main == null) ? tree.shadowGrid.getLightExposure(top) : main.Q;
            float lateral_Q = (lateral == null) ? tree.shadowGrid.getLightExposure(top) : lateral.Q;
            
            V += stored_V;
            
            float nom = V * tree.dna.ENERGY_LAMBDA * main_Q;
            float denom = tree.dna.ENERGY_LAMBDA * main_Q + (1 - tree.dna.ENERGY_LAMBDA) * lateral_Q;
            
            float ENERGY_MAIN = nom / denom;
            float ENERGY_LATERAL = V * (1 - tree.dna.ENERGY_LAMBDA) * lateral_Q / (tree.dna.ENERGY_LAMBDA * main_Q + (1 - tree.dna.ENERGY_LAMBDA) * lateral_Q);

            if (main == null) Grow(ENERGY_MAIN, true);
            else
            {
                main.V = ENERGY_MAIN;
                main.DistributeEnergy();
            }

            if (lateral == null) Grow(ENERGY_LATERAL, false);
            else
            {
                lateral.V = ENERGY_LATERAL;
                lateral.DistributeEnergy();
            }
        }
        

        public float CalculateBranchDiameter()
        {
            float d_squared_main = (main == null) ? Mathf.Pow(tree.dna.MIN_DIAMETER, tree.dna.BRANCH_DIAMTER_n) : Mathf.Pow(main.CalculateBranchDiameter(), tree.dna.BRANCH_DIAMTER_n);
            float d_squared_later = (lateral == null) ? Mathf.Pow(tree.dna.MIN_DIAMETER, tree.dna.BRANCH_DIAMTER_n) : Mathf.Pow(lateral.CalculateBranchDiameter(), tree.dna.BRANCH_DIAMTER_n);

            diameter = Mathf.Pow(d_squared_main + d_squared_later, 1.0f / tree.dna.BRANCH_DIAMTER_n);
            return diameter;
        }
        
        public int GetInternodes()
        {
            int internodes = 1;
            if (main == null) return internodes;
            else return internodes + main.GetInternodes();
        }

        public bool SheddBranches()
        {
            if ((Q/GetInternodes()) < tree.dna.SHEDDING_TRESHOLD) return true;

            if (main != null)
            {
                if (main.SheddBranches()) main = null;
            }
            if (lateral != null)
            {
                if (lateral.SheddBranches()) lateral = null;
            }

            return false;
        }
    }
}
