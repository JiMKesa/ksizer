namespace ksizer.Utils;

public static class Settings
{
    public static List<TMaterial> TankTexture;
    public static float[] Scaling;
    public static float[] ScalingTop;
    public static float[] ScalingCont;
    public static float[] ScalingRad;
    public static int[] NbMaterial;

    public static void Initialize()
    {
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
        NbMaterial = new int[] { 0, 7 };
    }
    
    public static float GetMassT(int modele, int wscale)
    {
        switch (modele)
        {
            case 1:
                //float[] tab = { 0.0017f, 0.0065f, 0.05f, 0.5f, 1.5f, 4f, 12.5f };
                //float[] tab = { 0.001f, 0.005f, 0.03f, 0.4f, 1.1f, 3.2f, 9.0f };
                float[] tab = { 0.000075f, 0.000425f, 0.0025f, 0.0325f, 0.0925f, 0.27f, 0.75f };
                return tab[wscale];
            default: throw new NotImplementedException();
        }
    }

    public static float GetMassC(int modele, int wscale)
    {
        switch (modele)
        {
            case 1:
                //float[] tab = { 0.0083f, 0.035f, 0.25f, 2.5f, 7.5f, 20f, 62.5f };
                //float[] tab = { 0.001f, 0.005f, 0.03f, 0.4f, 1.1f, 3.2f, 9.0f };
                float[] tab = { 0.00085f, 0.00415f, 0.025f, 0.335f, 0.916f, 2.66f, 7.5f };
                return tab[wscale];
            default: throw new NotImplementedException();
        }
    }

    public static float GetVolT(int modele, int wscale)
    {
        switch (modele)
        {
            case 1:
                float[] tab = { 0.0015f, 0.0075f, 0.58f, 1.7f, 4.7f, 15f };
                return tab[wscale];
            default: throw new NotImplementedException();
        }
    }
    public static float GetVolC(int modele, int wscale)
    {
        switch (modele)
        {
            case 1:
                float[] tab = { 0.0085f, 0.0375f, 0.25f, 2.9f, 8.35f, 23.5f, 75f };
                return tab[wscale];
            default: throw new NotImplementedException();
        }
    }
}
public enum FuelTypes
{
    monopropellant,
    solidfuel,
    intakeair,
    testrocks,
    evapropellant,
    hydrogen,
    methane,
    oxidizer,
    methalox,
    methaneair,
    uranium,
    electriccharge,
    xenon,
    xenonec,
    ablator,
}

public class TMaterial
{
    public int id { get; set; }
    public int idobject { get; set; }
    public string material { get; set; }
}

