namespace ksizer.Utils;

public static class Settings
{
    public static List<TMaterial> TankTexture;
    public static float[] Scaling;
    public static float[] ScalingTop;
    public static float[] ScalingCont;
    public static float[] ScalingRad;

    public static float[][] VolT;
    public static float[][] VolC;
    public static float[][] MassT;
    public static float[][] MassC;

    public static void Initialize()
    {
        K.Log("DEBUGLOG Initialize");
        // 
        TankTexture = new List<TMaterial>()
        {
           new TMaterial() { id = 0, idobject = 1, material = "ktank_1_1" },
           new TMaterial() { id = 1, idobject = 1, material = "ktank_1_2" }
        };
        // scaling ratio
        Scaling = new float[] { 0.03125f, 0.0625f, 0.125f, 0.25f, 0.375f, 0.5f, 1.0f };
        ScalingTop = new float[] { 0, 0.8f };
        ScalingCont = new float[] { 0, 1.6f };
        ScalingRad = new float[] { 0, 5f };

        // Volume Top/Bottom Tank
        float[][] VolT = new float[][]
        {
            new float[] { },
            new float[] { 0.0015f, 0.0075f, 0.58f, 1.7f, 4.7f, 15}
        };
        // Volume Center Tank
        float[][] VolC = new float[][]
        {
            new float[] { },
            new float[] { 0.0085f, 0.0375f, 0.25f, 2.9f, 8.35f, 23.5f, 75f}
        };
        // Mass Top/Bottom Tank
        float[][] MassT = new float[][]
        {
            new float[] { },
            new float[] { 0.0017f, 0.0065f, 0.005f, 0.5f, 1.5f, 4f, 12.5f}
        };
        // Mass Center Tank
        float[][] MassC = new float[][]
        {
            new float[] { },
            new float[] { 0.0083f, 0.035f, 0.25f, 2.5f, 7.5f, 20f, 62.5f}
        };

        K.Log("DEBUGLOG Initialize END");
    }
}

public class TMaterial
{
    public int id { get; set; }
    public int idobject { get; set; }
    public string material { get; set; }
}

