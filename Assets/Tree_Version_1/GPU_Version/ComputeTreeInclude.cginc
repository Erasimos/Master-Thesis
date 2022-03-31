
// Tree Parameters
float GROWTH_TRESHOLD = 1;
float PERCEPTION_ANGLE_LATERAL = 140;
float PERCEPTION_ANGLE_MAIN = 30;
float SEGMENT_LENGTH = 1;
static int NUM_TREES = 100;
static int batch_size = 1024;
static int batches = 100;
static int MAX_BRANCHES = batches * batch_size;

// PARAMETERS
float ENERGY_ALPHA = 5;
float ENERGY_LAMBDA = 0.5;