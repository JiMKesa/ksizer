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
    public int Model = 1;

    protected CorePartData CorePartData;
    private int OldModel = 1;
    private Material Material = null;
    private double Mass = 0.1;
    private string Resource = "Methalox";
    private double Capacity = 0.1;
    private double Initial = 0.1;
    private int OldScaleHeight;
    private int ActuScaleHeight;
    private IObjectAssemblyPartNode _floatingNodeB;
    private IObjectAssemblyPartNode _floatingNodeS;
    

    public override void AddDataModules()
    {
        base.AddDataModules();
        _data_SizerTank ??= new Data_SizerTank();
        DataModules.TryAddUnique(_data_SizerTank, out _data_SizerTank);
    }

    public override void OnInitialize()
    {
        base.OnInitialize();
        if (PartBackingMode == PartBackingModes.Flight)
        {
            this._data_SizerTank.SetVisible((IModuleDataContext)this._data_SizerTank.SliderScaleWidth, false);
            this._data_SizerTank.SetVisible((IModuleDataContext)this._data_SizerTank.SliderScaleHeight, false);
            OnFlyScaleWPart(this.part.FindModelTransform("AllTanks"), ScaleWidth);
            UpdateFlightPAMVisibility();
            OnFlyCreateContainer(ScaleHeight, Model);
        }
        else
        {
            this._data_SizerTank.SetVisible((IModuleDataContext)this._data_SizerTank.SliderScaleWidth, true);
            this._data_SizerTank.SetVisible((IModuleDataContext)this._data_SizerTank.SliderScaleHeight, true);
            OldScaleHeight = ScaleHeight;
            OnOABScaleWPart(this.OABPart.PartTransform.FindChildRecursive("AllTanks"), ScaleWidth);
            OnOABAdjustNodeAttach((float)ScaleWidth, Model);
            UpdateOabPAMVisibility();
            //this._data_SizerTank.SliderScaleWidth.OnChanged += new Action(this.OnOABScaleWidthChanged);
            this._data_SizerTank.SliderScaleWidth.OnChangedValue += new Action<float>(this.OnOABScaleWidthChanged);
            //this._data_SizerTank.SliderScaleHeight.OnChanged += new Action(this.OnOABScaleHeightChanged);
            this._data_SizerTank.SliderScaleHeight.OnChangedValue += new Action<float>(this.OnOABScaleHeightChanged);
        }
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
        OnOABScaleWPart(this.OABPart.PartTransform.FindChildRecursive("AllTanks"), ScaleWidth);
        //OnOABAdjustNodeAttach((float)ScaleWidth, Model);
        OnOABAdjustNodeAttach(ScaleH, Model);
    }

    private void OnOABScaleHeightChanged(float ScaleH)
    {
        K.Log("DEBUGLOG OnOABScaleHeightChanged");
        OnOABCreateContainer(ScaleH, Model);
    }
    public void OnFlyCreateContainer(float ScaleH, int modele)
    {
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
                // ---------
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
        string _TankNode = "Tank_" + modele.ToString("0");
        string _namecopy = "Container_" + modele.ToString("0") + "_1";
        string _namebottom = "Bottom_" + modele.ToString("0");
        var _TransformTank_1 = this.OABPart.PartTransform.FindChildRecursive(_TankNode);
        if (_TransformTank_1 != null)
        {
            for (int cpt = 1; cpt < (int)ScaleH; cpt++)
            {
                string _newname = "Container_" + modele.ToString("0") + "_" + (cpt + 1).ToString("0");
                var _PartToCopy = this.OABPart.PartTransform.FindChildRecursive(_namecopy).gameObject;
                if ((_PartToCopy != null) && (this.OABPart.PartTransform.FindChildRecursive(_newname) == null))
                {
                    GameObject gameObjnew = UnityEngine.Object.Instantiate<GameObject>(_PartToCopy);
                    gameObjnew.name = _newname;
                    gameObjnew.transform.parent = _TransformTank_1;
                    float newz = -(cpt * Settings.ScalingCont[modele]);
                    gameObjnew.transform.localPosition = new Vector3(0f, 0f, newz);
                    gameObjnew.transform.localRotation = _PartToCopy.transform.localRotation;
                    gameObjnew.transform.localScale = _PartToCopy.transform.localScale;
                    // ---------
                    var _PartBottom = this.OABPart.PartTransform.FindChildRecursive(_namebottom);
                    if (_PartBottom != null)
                    {
                        float newbottomz = -(((cpt+1) * Settings.ScalingCont[modele]) + Settings.ScalingTop[modele]);
                        _PartBottom.localPosition = new Vector3(0f, 0f, newbottomz);
                    }
                    OnOABAdjustNodeAttach((float)ScaleWidth, modele);
                }
            }
            for (int cpt = 20; cpt > (int)ScaleH; cpt--)
            {
                string _delname = "Container_" + modele.ToString("0") + "_" + cpt.ToString("0");
                if ((this.OABPart.PartTransform.FindChildRecursive(_delname) != null))
                {
                    var _PartToDel = this.OABPart.PartTransform.FindChildRecursive(_delname).gameObject;
                    _PartToDel.DestroyGameObject();
                    // ---------
                    var _PartBottom = this.OABPart.PartTransform.FindChildRecursive(_namebottom);
                    if (_PartBottom != null)
                    {
                        float newbottomz = -(((cpt-1) * Settings.ScalingCont[modele]) + Settings.ScalingTop[modele]);
                        _PartBottom.localPosition = new Vector3(0f, 0f, newbottomz);
                    }
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


    public override void OnShutdown()
    {
        K.Log("DEBUGLOG OnShutdown");

    }

    private void UpdateFlightPAMVisibility()
    {
        //_data_SizerTank.SetVisible(_Data_SpaceObs.EnabledToggle, true);
    }
    private void UpdateOabPAMVisibility()
    {
        //_data_SizerTank.SetVisible(_Data_SpaceObs.EnabledToggle, false);
    }
}
