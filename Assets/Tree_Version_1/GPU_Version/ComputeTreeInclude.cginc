
// Tree Parameters
float PERCEPTION_ANGLE_LATERAL = 140;
float PERCEPTION_ANGLE_MAIN = 30;
float SEGMENT_LENGTH = 1;
static int NUM_TREES = 250;
static int MAX_BRANCHES = 10000000;
static int batch_size = 1024;
static int batches = MAX_BRANCHES / batch_size;

// PARAMETERS
float ENERGY_ALPHA = 5;
static int DIRECTIONS_SAMPLES = 10;
static float GROWTH_THRESHOLD = 0.5;
static float MAIN_BRANCHING_ANGLE = radians(10);
static float LATERAL_BRANCHING_ANGLE = radians(60);
static float MIN_BRANCH_DIAMETER = 0.1;
static float3 GRAVITY = float3(0, 1, 0);
static float GRAVITROPISM = 0.5;
