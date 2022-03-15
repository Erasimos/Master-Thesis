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

    public Tree(TreeDNA dna, ShadowGrid shadowGrid)
    {
        
        stop = false;
        this.dna = dna;
        this.shadowGrid = shadowGrid;
        age = 0;
        Branch root = new Branch(new Vector3(0, 0, 0), new Vector3(0, 8, 0), this, shadowGrid, 1);
        this.root = root;
        this.newBranches = new List<Branch>();
        newBranches.Add(root);
        grown = true;
    }

    public void Grow()
    {
        age += 1;
        if (age > dna.MAX_AGE) stop = true;
        root.GatherEnergy();
        root.V = root.Q * dna.ENERGY_COEEFICENT;
        root.DistributeEnergy();
        root.SecondaryGrowth(new Vector3(0, 0, 0));
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
        public List<Branch> laterals;
        public List<Bud> buds;
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
            laterals = new List<Branch>();
            direction = Vector3.Normalize(top - bottom);
            this.tree = tree;
            shadowGrid.updateGrid(top);
            this.depth = depth;
            growing = true;
            GenerateBuds();
        }

        public void GenerateBuds()
        {
            buds = new List<Bud>();
            int numberOfBuds = MathHelper.GetRandom(tree.dna.MIN_BUDS_PER_SEGMENT, tree.dna.MAX_BUDS_PER_SEGMENT);
            float branchLength = (top - bottom).magnitude;

            buds.Add(new Bud(this, top));

            for (int i = 1; i < numberOfBuds; i++)
            {
                float offset = MathHelper.GetRandom(tree.dna.BUD_SPREAD, 1f);
                Vector3 budPosition = bottom + direction * branchLength * offset;
                buds.Add(new Bud(this, budPosition));
            }
        }

        public float GatherEnergy()
        {
            float Energy = 0;

            // Gather energy from existing children branches
            if (main != null) Energy += main.GatherEnergy();
            foreach (Branch lateral in laterals) Energy += lateral.GatherEnergy();

            // Gather energy from buds
            foreach (Bud bud in buds) Energy += bud.GatherEnergy();

            Q = Energy;
            return Q;
        }

        public void DistributeEnergy()
        {

            if (buds.Count != 0)
            {
                Bud[] currentBuds = new Bud[buds.Count];
                buds.CopyTo(currentBuds);
                foreach (Bud bud in currentBuds) bud.Grow(V / currentBuds.Length);
                return;
            }
            else if (main == null) return;

            int numberOfLaterals = laterals.Count;
            float main_Q = main.Q;
            float lateral_Q = 0;
            foreach (Branch lateral in laterals) lateral_Q += lateral.Q;
            
            this.V += this.stored_V;
            
            float nom = this.V * tree.dna.ENERGY_LAMBDA * main_Q;
            float denom = tree.dna.ENERGY_LAMBDA * main_Q + (1 - tree.dna.ENERGY_LAMBDA) * lateral_Q;
            
            float ENERGY_MAIN = nom / denom;
            float ENERGY_LATERAL = this.V * (1 - tree.dna.ENERGY_LAMBDA) * lateral_Q / (tree.dna.ENERGY_LAMBDA * main_Q + (1 - tree.dna.ENERGY_LAMBDA) * lateral_Q);

            main.V = ENERGY_MAIN;
            main.DistributeEnergy();

            if (numberOfLaterals == 0)
            {
                foreach (Bud bud in buds) bud.Grow(ENERGY_LATERAL / buds.Count);
            }
            else
            {
                foreach (Branch lateral in laterals)
                {
                    lateral.V = ENERGY_LATERAL / numberOfLaterals;
                    lateral.DistributeEnergy();
                }
            }
        }
        

        public float CalculateBranchDiameter()
        {
            if (main == null)
            {
                diameter = tree.dna.MIN_DIAMETER;
                return tree.dna.MIN_DIAMETER;
            }
            else
            {
                float d_squared = Mathf.Pow(main.CalculateBranchDiameter(), tree.dna.BRANCH_DIAMTER_n);
                
                if (laterals.Count != 0) foreach (Branch lateral in laterals) d_squared += Mathf.Pow(lateral.CalculateBranchDiameter(), tree.dna.BRANCH_DIAMTER_n);
                else d_squared += Mathf.Pow(tree.dna.MIN_DIAMETER, tree.dna.BRANCH_DIAMTER_n);
                diameter = Mathf.Pow(d_squared, 1.0f / tree.dna.BRANCH_DIAMTER_n);
                return this.diameter;
            }
        }

        public void SecondaryGrowth(Vector3 offset)
        {
            bottom += offset;
            Vector3 newOffset = offset += tree.dna.AGE_WEIGHT * direction;
            top += newOffset;

            foreach (Bud bud in buds) bud.position += newOffset;

            if (main == null) return;

            main.SecondaryGrowth(newOffset);
            foreach (Branch lateral in laterals)
            {
                lateral.SecondaryGrowth(newOffset);
            }

        }
    }

    public class Bud
    {
        private Branch branch;
        private float energy;
        public Vector3 position;

        public Bud(Branch branch, Vector3 position)
        {
            this.branch = branch;
            this.position = position;
            energy = 0;
        }

        public void Grow(float energy)
        {
            this.energy += energy;
            float angle = branch.tree.dna.PERCEPTION_ANGLE;

            if (branch.main == null) angle *= 0.2f;

            if (this.energy >= branch.tree.dna.SPROUT_ENERGY)
            {
                Vector3 growthDirection = findOptimalGrowthDirectionV2(angle);
                while(growthDirection == new Vector3(0, 0, 0))
                {
                    this.energy *= 0.5f;
                    growthDirection = findOptimalGrowthDirectionV2(angle);

                    if (this.energy <= branch.tree.dna.BUD_DEATH_TRESHOLD)
                    {
                        branch.buds.Remove(this);
                        return;
                    }
                }

                Vector3 newTop = position + growthDirection * getBranchLength();
                Branch newBranch = new Branch(position, newTop, branch.tree, branch.tree.shadowGrid, branch.depth + 1);
                if (branch.top == position) branch.main = newBranch;
                else branch.laterals.Add(newBranch);
                branch.buds.Remove(this);
                branch.tree.newBranches.Remove(branch);
                branch.tree.newBranches.Add(newBranch);
            }
        }

        public float getBranchLength()
        {
            return energy;
        }

        public Vector3 findOptimalGrowthDirectionV2(float angle)
        {
            Vector3 optimalDirection = new Vector3(0, 0, 0);
            float minShadowvalue = Mathf.Infinity;

            for (int i = 0; i < branch.tree.dna.DIRECTION_SAMPLES; i++)
            {
                Vector3 sampleDirection = MathHelper.SamplePerceptionSphere(branch.direction, angle) + new Vector3(0, 1, 0) * branch.tree.dna.GRAVITROPISM_WIEGHT + branch.direction * branch.tree.dna.SELFTROPISM_WEIGHT;
                float sampleStep = getBranchLength() / branch.tree.dna.SHADOW_SAMPLES;
                float shadowValue = 0;

                for (int j = 0; j < branch.tree.dna.SHADOW_SAMPLES; j++)
                {
                    Vector3 samplePos = position + sampleDirection * sampleStep * j;
                    shadowValue += branch.tree.shadowGrid.getShadowValuePos(samplePos);
                }

                if (shadowValue < minShadowvalue)
                {
                    minShadowvalue = shadowValue;
                    optimalDirection = sampleDirection;
                }
            }
            return optimalDirection.normalized;
        }

        public float GatherEnergy()
        {
            return branch.tree.shadowGrid.getLightExposure(position);
        }
    }
}
