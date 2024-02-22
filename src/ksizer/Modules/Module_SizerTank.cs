using KSP.Sim.Definitions;
using UnityEngine;

using KSP;
using UnityEngine.Serialization;
using KSP.Sim;
using KSP.OAB;
using KSP.Game;
using KSP.Sim.ResourceSystem;

using ksizer.Utils;
using UnityEngine.UIElements.StyleSheets;
using KSP.UI.Binding;
using Moq;
using System.Collections.Generic;
using UnityEngine.UIElements;
using static KSP.Api.UIDataPropertyStrings.View.Vessel.Stages;
using System.Collections;

namespace ksizer.Modules;

[DisallowMultipleComponent]

public class Module_SizerTank : PartBehaviourModule
{
    public override Type PartComponentModuleType => typeof(PartComponentModule_SizerTank);
    
    [FormerlySerializedAs("data")]
    [SerializeField]
    protected Data_SizerTank _data_SizerTank;
    [SerializeField]
    public int ScaleWidth => (int)this._data_SizerTank.SliderScaleWidth.GetValue();
    [SerializeField]
    public int ScaleHeight => (int)this._data_SizerTank.SliderScaleHeight.GetValue();
    [SerializeField]
    public string ResourcesList => (string)this._data_SizerTank.ResourcesList.GetValue();

    [SerializeField]
    public int Model = 1;

    protected CorePartData CorePartData;
    private int OldModel = 1;
    private Material Material = null;
    private double Mass = 0.1;
    private float _panelMass;
    public int idresource = 7;
    private double Capacity = 0.1;
    private double Initial = 0.1;
    private int ActuScaleHeight;
    private IObjectAssemblyPartNode _floatingNodeB;
    private IObjectAssemblyPartNode _floatingNodeS;
    private float _deltaUniverseTime;
    private OABSessionInformation _stats;
    // TEST
    // Game.OAB.Current.Stats.MainAssembly. !!!!!!!!!!!!!!
    // -----------------------------------------------------------------------------
    /*
    private OABSessionInformation _stats;
    private ObjectAssemblyEngineerReport _EngineerReport;
    protected ObjectAssemblyBuilderEvents _events;
    public OABSessionInformation Stats => this._stats;
    
    private ObjectAssemblyPartTracker _oabPartTracker;
    private ObjectAssemblyPart _oabAssPart;

    private ObjectAssemblyEngineerReport engineerReport;
    private ObjectAssemblyBuilderEvents events;
    private EngineeringReportFlawListSetup flawListSetup;
    private ObjectAssemblyBuilder builder;
    */
    // -----------------------------------------------------------------------------

    public override void AddDataModules()
    {
        base.AddDataModules();
        _data_SizerTank ??= new Data_SizerTank();
        DataModules.TryAddUnique(_data_SizerTank, out _data_SizerTank);
    }
    
    /*
     * Module initialization
    */
    public override void OnInitialize()
    {
        base.OnInitialize();
        // Check Fly mode Vs OAB mode
        if (PartBackingMode == PartBackingModes.Flight)
        {
            // hide PAM config 
            this._data_SizerTank.SetVisible((IModuleDataContext)this._data_SizerTank.SliderScaleWidth, false);
            this._data_SizerTank.SetVisible((IModuleDataContext)this._data_SizerTank.SliderScaleHeight, false);
            this._data_SizerTank.SetVisible((IModuleDataContext)this._data_SizerTank.ResourcesList, false);
            // Scale width tank
            OnFlyScaleWPart(this.part.FindModelTransform("AllTanks"), ScaleWidth);
            // Scale Height tank
            OnFlyCreateContainer(ScaleHeight, Model);
        }
        else
        {
            this._stats = Game.OAB.Current.ActivePartTracker.stats;
            // show PAM config 
            this._data_SizerTank.SetVisible((IModuleDataContext)this._data_SizerTank.SliderScaleWidth, true);
            this._data_SizerTank.SetVisible((IModuleDataContext)this._data_SizerTank.SliderScaleHeight, true);
            this._data_SizerTank.SetVisible((IModuleDataContext)this._data_SizerTank.ResourcesList, true);
            // scale Width Tank
            OnOABScaleWPart(this.OABPart.PartTransform.FindChildRecursive("AllTanks"), ScaleWidth);
            // update Tank mass
            MassModifier(ScaleWidth, ScaleHeight, Model);
            // replace nodes (it depends of tank width & height)
            OnOABAdjustNodeAttach((float)ScaleWidth, Model);
            // Actions when PAM sliders change
            this._data_SizerTank.SliderScaleWidth.OnChangedValue += new Action<float>(this.OnOABScaleWidthChanged);
            this._data_SizerTank.SliderScaleHeight.OnChangedValue += new Action<float>(this.OnOABScaleHeightChanged);
            this._data_SizerTank.ResourcesList.OnChangedValue += new Action<string>(this.OnOABSResourceChanged);
            // update vessel informations for Engineer report
            UpdateVesselInfo();
        }
    }

    // Update vessel information for engineer report windows
    public void UpdateVesselInfo()
    {
        Game.OAB.Current.ActivePartTracker.stats.engineerReport.UpdateReport(this._stats);
    }

    public void UpdateResources()
    {

    }

    // scale the part in OAB windows
    private void OnOABScaleWPart(Transform _Part, int Scalevalue)
    {
        _Part.localScale = new Vector3(Settings.Scaling[Scalevalue], Settings.Scaling[Scalevalue], Settings.Scaling[Scalevalue]);
    }

    // scale the part in Fly windows
    private void OnFlyScaleWPart(Transform _Part, int Scalevalue)
    {
        _Part.localScale = new Vector3(Settings.Scaling[ScaleWidth], Settings.Scaling[ScaleWidth], Settings.Scaling[ScaleWidth]);
    }

    // Slider part Width change -> action
    private void OnOABScaleWidthChanged(float ScaleH)
    {
        // Width scale of (all) Tanks parts
        OnOABScaleWPart(this.OABPart.PartTransform.FindChildRecursive("AllTanks"), (int)ScaleH);
        // update tank mass
        MassModifier((int)ScaleH, ScaleHeight, Model);
        // replace nodes (it depends of tank width & height)
        OnOABAdjustNodeAttach(ScaleH, Model);
        // update vessel information for engineer report
        UpdateVesselInfo();
    }

    private void OnOABScaleHeightChanged(float ScaleH)
    {
        // create new tank container part -> height change
        OnOABCreateContainer(ScaleH, Model);
        // update tank mass
        MassModifier(ScaleWidth, (int)ScaleH, Model);
        // update vessel information for engineer report
        UpdateVesselInfo();
    }

    private void OnOABSResourceChanged(string Resourcechoice)
    {
        // -> this.OABPart.AvailablePart.PartData.resourceContainers.Count();

        string ResourceLabel = Enum.GetName(typeof(FuelTypes), Int32.Parse(Resourcechoice));
        //K.Log("DEBUGLOG OnOABSResourceChanged :"+ Resourcechoice + " : " + ResourceLabel);
        this.idresource = Int32.Parse(Resourcechoice);


        //this.Resource = Resourcechoice;
        //this.OABPart.AvailablePart.PartData.resourceContainers[0].name = Resourcechoice;
        //K.Log("DEBUGLOG RESSOURCES 1: " + this.OABPart.Containers[0].Count());
        //K.Log("DEBUGLOG RESSOURCES 1a: " + this.OABPart.AvailablePart.PartData.resourceContainers.Count());
        //K.Log("DEBUGLOG RESSOURCES 1b: " + this.OABPart.AvailablePart.PartData.resourceContainers[0].name);
        //this.OABPart.AvailablePart.PartData.resourceContainers.Clear();
        //K.Log("DEBUGLOG RESSOURCES 1c: " + this.OABPart.AvailablePart.PartData.resourceContainers.Count());
        //
        //this.OABPart.AvailablePart.ResourceContainers.
        //var tr = this.OABPart.Containers[0].AsEnumerable<ResourceDefinitionID>;
        //this.OABPart.Containers[0].GetEnumerator().MoveNext();
        //K.Log("DEBUGLOG RESSOURCES 2: " + this.OABPart.Containers[0].Count());
    }
    public void OnFlyCreateContainer(float ScaleH, int modele)
    {
        // using tank model choice (int modele) to catch gameobject part for rebuild tank
        string _TankNode = "Tank_" + modele.ToString("0");
        string _namecopy = "Container_" + modele.ToString("0") + "_1";
        string _namebottom = "Bottom_" + modele.ToString("0");
        var _TransformTank_1 = this.part.FindModelTransform(_TankNode);
        if (_TransformTank_1 != null)
        {
            for (int cpt = 1; cpt < (int)ScaleH; cpt++)
            {
                string _newname = "Container_" + modele.ToString("0") + "_" + (cpt + 1).ToString("0");
                var _PartToCopy = this.part.FindModelTransform(_namecopy).gameObject;
                // containers
                if ((_PartToCopy != null) && (this.part.FindModelTransform(_newname) == null))
                {
                    GameObject gameObjnew = UnityEngine.Object.Instantiate<GameObject>(_PartToCopy);
                    gameObjnew.name = _newname;
                    gameObjnew.transform.parent = _TransformTank_1;
                    float newz = -(cpt * Settings.ScalingCont[modele]);
                    gameObjnew.transform.localPosition = new Vector3(0f, 0f, newz);
                    gameObjnew.transform.localRotation = _PartToCopy.transform.localRotation;
                    gameObjnew.transform.localScale = _PartToCopy.transform.localScale;
                }
                // bottom of tank (last part) -> place it at bottom of all containers
                var _PartBottom = this.part.FindModelTransform(_namebottom);
                if (_PartBottom != null)
                {
                    float newbottomz = -(((cpt + 1) * Settings.ScalingCont[modele]) + Settings.ScalingTop[modele]);
                    _PartBottom.localPosition = new Vector3(0f, 0f, newbottomz);
                }
            }
        }
    }
    // Tank height size changing
    public void OnOABCreateContainer(float ScaleH, int modele)
    {
        // using tank model choice (int modele) to catch gameobject part for rebuild tank
        string _TankNode = "Tank_" + modele.ToString("0");
        string _namecopy = "Container_" + modele.ToString("0") + "_1";
        string _namebottom = "Bottom_" + modele.ToString("0");
        var _TransformTank_1 = this.OABPart.PartTransform.FindChildRecursive(_TankNode);
        if (_TransformTank_1 != null)
        {
            // height 1 to slider value
            for (int cpt = 1; cpt < (int)ScaleH; cpt++)
            {
                string _newname = "Container_" + modele.ToString("0") + "_" + (cpt + 1).ToString("0");
                var _PartToCopy = this.OABPart.PartTransform.FindChildRecursive(_namecopy).gameObject;
                if ((_PartToCopy != null) && (this.OABPart.PartTransform.FindChildRecursive(_newname) == null))
                {
                    // containers
                    GameObject gameObjnew = UnityEngine.Object.Instantiate<GameObject>(_PartToCopy);
                    gameObjnew.name = _newname;
                    gameObjnew.transform.parent = _TransformTank_1;
                    float newz = -(cpt * Settings.ScalingCont[modele]);
                    gameObjnew.transform.localPosition = new Vector3(0f, 0f, newz);
                    gameObjnew.transform.localRotation = _PartToCopy.transform.localRotation;
                    gameObjnew.transform.localScale = _PartToCopy.transform.localScale;
                    // bottom of tank(last part)->place it at bottom of all containers
                    var _PartBottom = this.OABPart.PartTransform.FindChildRecursive(_namebottom);
                    if (_PartBottom != null)
                    {
                        float newbottomz = -(((cpt+1) * Settings.ScalingCont[modele]) + Settings.ScalingTop[modele]);
                        _PartBottom.localPosition = new Vector3(0f, 0f, newbottomz);
                    }
                    // adjust vessel Attach nodes
                    OnOABAdjustNodeAttach((float)ScaleWidth, modele);
                }
            }
            // removing all containers after Height slider value
            for (int cpt = 30; cpt > (int)ScaleH; cpt--)
            {
                string _delname = "Container_" + modele.ToString("0") + "_" + cpt.ToString("0");
                if ((this.OABPart.PartTransform.FindChildRecursive(_delname) != null))
                {
                    // containers
                    var _PartToDel = this.OABPart.PartTransform.FindChildRecursive(_delname).gameObject;
                    _PartToDel.DestroyGameObject();
                    // bottom of tank(last part)->place it at bottom of all created containers
                    var _PartBottom = this.OABPart.PartTransform.FindChildRecursive(_namebottom);
                    if (_PartBottom != null)
                    {
                        float newbottomz = -(((cpt-1) * Settings.ScalingCont[modele]) + Settings.ScalingTop[modele]);
                        _PartBottom.localPosition = new Vector3(0f, 0f, newbottomz);
                    }
                    // adjust vessel Attach nodes
                    OnOABAdjustNodeAttach((float)ScaleWidth, modele);
                }

            }
        }
        else
        {
            K.Log("DEBUGLOG Tank_1 NOT found");
        }
    }

    // calculate AttachNode with Scale in OAB
    public void OnOABAdjustNodeAttach(float scalewidth, int modele)
    {
        _floatingNodeB = this.OABPart.FindNodeWithTag("bottom");
        _floatingNodeS = this.OABPart.FindNodeWithTag("srfAttach");
        // Bottom AttachNode
        if (_floatingNodeB != null)
        {
            float TotalCont = (float)ScaleHeight * Settings.ScalingCont[modele];
            float newy = -((2 * Settings.ScalingTop[modele]) + TotalCont) * Settings.Scaling[(int)scalewidth];
            var nodv = new Vector3(0f, newy, 0f);
            //K.Log("DEBUGLOG Y:" + _floatingNodeB.NodeTransform.localPosition.y);
            this.OABPart.SetNodeLocalPosition(_floatingNodeB, nodv);
        }
        // Surface AttachNode
        if (_floatingNodeS != null)
        {
            float newy = -(((2 * Settings.ScalingTop[modele]) + Settings.ScalingCont[modele]) * 0.5f * Settings.Scaling[(int)scalewidth]);
            float newz = Settings.ScalingRad[modele] * Settings.Scaling[(int)scalewidth];
            var nodv = new Vector3(0f, newy, newz);
            this.OABPart.SetNodeLocalPosition(_floatingNodeS, nodv);
        }
    }

    public void MassModifier(int wscale, int hscale, int modele)
    {
        K.Log("------------------------------------------------------------------");
        //this.mass = this.AvailablePart.PartData.mass + this.massModifyAmount;
        K.Log("DEBUGLOG MASS modele:" + modele + " | scale:" + wscale + " -> 2*" + Settings.GetMassT(modele, wscale));
        this._panelMass = (2 * Settings.GetMassT(modele,wscale)); 
        for (int i=1; i<=ScaleHeight; i++)
        {
            K.Log("DEBUGLOG MASS container(" + i + ") : " + Settings.GetMassC(modele, wscale));
            this._panelMass += Settings.GetMassC(modele, wscale);
        }
        K.Log("DEBUGLOG mass calcul:" + this._panelMass);
        this._data_SizerTank.DryMass = this._panelMass;
        this._data_SizerTank.mass = this._panelMass;


        this.OABPart.AvailablePart.PartData.mass = this._panelMass;
        (this.OABPart as ObjectAssemblyPart).mass = this._panelMass;
        (this.OABPart as ObjectAssemblyPart).UpdateMassValues();

        

    }

    public void VesselMassUpdate()
    {

    }
    public void VesselSizeUpdate()
    {
        /*
        // ObjectAssembly.SetAssemblyBounds()
        Bounds bbox = this.OABPart.Assembly.GetBoundingBox();
        K.Log("DEBUGLOG BBOX x:" + bbox.size.x);
        K.Log("DEBUGLOG BBOX y:" + bbox.size.y);
        K.Log("DEBUGLOG BBOX z:" + bbox.size.z);
        K.Log("DEBUGLOG MASS TOTAL:" + this.OABPart.Assembly.GetTotalMass());
        */
    }

    public override void OnUpdate(float deltaTime)
    {
        this._deltaUniverseTime = deltaTime;
    }

    // This triggers in OAB
    public override void OnModuleOABFixedUpdate(float deltaTime)
    {

    }

    public override void OnShutdown()
    {
        K.Log("DEBUGLOG OnShutdown");

    }
}
