﻿using KSP.Sim.Definitions;
using UnityEngine;
using KSP;
using UnityEngine.Serialization;
using KSP.Sim;
using KSP.OAB;
using KSP.Game;
using KSP.Sim.ResourceSystem;
using ksizer.Utils;
using KSP.Modules;
using KSP.Sim.impl;
using KSP.UI.Binding;
using SpaceWarp.API.Assets;
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
    public int ScaleWidth => Int32.Parse(this._data_SizerTank.SliderScaleWidth.GetValue());
    public int OldScaleWidth;
    public bool ScalingW = false;
    [SerializeField]
    public int ScaleHeight => Int32.Parse(this._data_SizerTank.SliderScaleHeight.GetValue());
    public int OldScaleHeight;
    public bool ScalingH = false;
    [SerializeField]
    public int Material => Int32.Parse(this._data_SizerTank.SliderMaterial.GetValue());
    //[SerializeField]
    //public string ResourcesList => (string)this._data_SizerTank.ResourcesList.GetValue();
    [SerializeField]
    public int Model = 1;
    // ---- ID Part ----------
    IGGuid PartIGGuid => (this.OABPart as ObjectAssemblyPart).GlobalId;
    // ---- stats engineer ---
    private OABSessionInformation _stats;
    // ---- part -------------
    protected CorePartData CorePartData;
    private int OldModel = 1;
    private float _panelMass;
    // ---- nodes ------------
    private IObjectAssemblyPartNode _floatingNodeT;
    private IObjectAssemblyPartNode _floatingNodeB;
    private IObjectAssemblyPartNode _floatingNodeS;
    // ---- colliders --------
    public Collider[] Colliders;
    // ---- resources --------_partsManagerCategoryPrefabPool
    public int idresource = 8;
    ResourceDefinitionID resourceId;
    ResourceDefinitionDatabase definitionDatabase => GameManager.Instance.Game.ResourceDefinitionDatabase;
    ResourceDefinitionData definitionData;
    public ResourceContainer Container = new KSP.Sim.ResourceSystem.ResourceContainer();
    private float Resourcevolume;
    // ---- Colors part ---------
    public Module_Color _moduleColor;
    (Color, Color) colors1;
    public override void AddDataModules()
    {
        base.AddDataModules();
        _data_SizerTank ??= new Data_SizerTank();
        DataModules.TryAddUnique(_data_SizerTank, out _data_SizerTank);
    }
    
    // Module initialization
    public override void OnInitialize()
    {
        base.OnInitialize();
        // Check Fly mode Vs OAB mode
        if (PartBackingMode == PartBackingModes.Flight)
        {
            // hide PAM config 
            this._data_SizerTank.SetVisible((IModuleDataContext)this._data_SizerTank.SliderScaleWidth, false);
            this._data_SizerTank.SetVisible((IModuleDataContext)this._data_SizerTank.SliderScaleHeight, false);
            this._data_SizerTank.SetVisible((IModuleDataContext)this._data_SizerTank.SliderMaterial, false);
            //this._data_SizerTank.SetVisible((IModuleDataContext)this._data_SizerTank.ResourcesList, false);
            // Scale width tank
            OnFlyScaleWPart(this.part.FindModelTransform("AllTanks"), ScaleWidth);
            // Scale Height tank
            OnFlyCreateContainer(ScaleHeight, Model);
        }
        else
        {
            // Set ResourceId to Methalox ID
            this.resourceId = GameManager.Instance.Game.ResourceDefinitionDatabase.GetResourceIDFromName(Enum.GetName(typeof(FuelTypes), idresource));
            // show PAM config 
            this._data_SizerTank.SetVisible((IModuleDataContext)this._data_SizerTank.SliderScaleWidth, true);
            this._data_SizerTank.SetVisible((IModuleDataContext)this._data_SizerTank.SliderScaleHeight, true);
            this._data_SizerTank.SetVisible((IModuleDataContext)this._data_SizerTank.SliderMaterial, true);
            //this._data_SizerTank.SetVisible((IModuleDataContext)this._data_SizerTank.ResourcesList, true);
            // Init Scale values backup
            this.OldScaleWidth = this.ScaleWidth;
            this.OldScaleHeight = this.ScaleHeight;
            // colors part
            this._moduleColor = this.GetComponent<Module_Color>();
            // scale Width Tank
            OnOABScaleWPart(this.OABPart.PartTransform.FindChildRecursive("AllTanks"), ScaleWidth);
            OnOABScaleWidthtNode(this.ScaleWidth, this.Model);
            // scale height Tank
            OnOABScaleHPart(ScaleHeight, Model);
            // Assign material
            AssignMaterial(Model, Material);
            // update Tank mass
            MassModifier(ScaleWidth, ScaleHeight, Model, idresource);
            // replace nodes (it depends of tank width & height)
// OnOABAdjustNodeAttach((float)ScaleWidth, Model);
            // Final ajust Parts
//AdjustFinalPart();
//AjustSymetricPart();
            // Actions when PAM sliders change
            this._data_SizerTank.SliderScaleWidth.OnChanged += new Action(this.SliderScaleWidthAction);
            this._data_SizerTank.SliderScaleHeight.OnChanged += new Action(this.SliderScaleHeightAction);
            this._data_SizerTank.SliderMaterial.OnChanged += new Action(this.SliderMaterialAction);
            //this._data_SizerTank.ResourcesList.OnChangedValue += new Action<string>(this.OnOABSResourceChanged);
            // update vessel informations for Engineer report
            UpdateVesselInfo();
            RefreshTank();
            DropDown("material", Model);
        }
    }
    // dynamicly create dropdown module property
    public void DropDown(string type, int idref)
    {
        switch (type)
        {
            case "material":
                var MaterialList = new DropdownItemList();
                for (int cpt=1; cpt <= Settings.NbMaterial[idref]; ++cpt)
                {
                    MaterialList.Add(cpt.ToString(), new DropdownItem() { key = cpt.ToString(), text = cpt.ToString() });
                }
                _data_SizerTank.SetDropdownData(_data_SizerTank.SliderMaterial, MaterialList);
                break;
        }
    }
    
    // Catch dropdown change 
    public void SliderScaleWidthAction()
    {
        this.ScalingW = true;
        OnOABScaleWidthChanged((float)this.ScaleWidth);
        this.OldScaleWidth = this.ScaleWidth;
        this.ScalingW = false;
    }
    public void SliderScaleHeightAction()
    {
        this.ScalingH = true;
        OnOABScaleHeightChanged((float)this.ScaleHeight);
        this.OldScaleHeight = this.ScaleHeight;
        this.ScalingH = false;
    }
    public void SliderMaterialAction()
    {
        AssignMaterial(this.Model, this.Material);
    }
    // assign material to tank
    public void AssignMaterial(int id_model, int id_material)
    {
        this.colors1 = this._moduleColor.GetColors();
        string MaterialName = "ktank_" + this.Model.ToString() + "_" + id_material.ToString() + ".mat";
        var newmat = AssetManager.GetAsset<Material>($"{KsizerPlugin.ModGuid}/ksizer_materials/mod/ktank/materials/{MaterialName}");
        string _name1 = "Top_" + this.Model.ToString("0");// + "_1";
        var _obj1 = this.OABPart.PartTransform.FindChildRecursive(_name1);
        _obj1.GetComponent<Renderer>().material = newmat;
        this.OABPart.ApplyColors(this.colors1.Item1, this.colors1.Item2);
        for (int cpt = 1; cpt <= this.ScaleHeight; ++cpt)
        {
            string _name = "Container_" + this.Model.ToString("0") + "_" + cpt.ToString();
            var _obj = this.OABPart.PartTransform.FindChildRecursive(_name);
            _obj.GetComponent<Renderer>().material = newmat;
            this.OABPart.ApplyColors(this.colors1.Item1, this.colors1.Item2);
        }
        string _name2 = "Bottom_" + this.Model.ToString("0");// + "_1";
        var _obj2 = this.OABPart.PartTransform.FindChildRecursive(_name2);
        _obj2.GetComponent<Renderer>().material = newmat;
        this.OABPart.ApplyColors(this.colors1.Item1, this.colors1.Item2);
    }
    // Update vessel information for engineer report windows
    public void UpdateVesselInfo()
    {
        if (GameManager.Instance.Game.PartsManager.IsVisible)
        {
            Game.OAB.Current.ActivePartTracker.stats.engineerReport.UpdateReport(Game.OAB.Current.ActivePartTracker.stats);
        }
    }

    // Tank height size changing
    public void OnOABScaleHPart(float ScaleH, int modele)
    {
        // using tank model choice (int modele) to catch gameobject part for rebuild tank
        string _TankNode = "Tank_" + modele.ToString("0");
        string _namecopy = "Container_" + modele.ToString("0") + "_1";
        string _namebottom = "Bottom_" + modele.ToString("0");
        string _namecollider = "col_" + modele.ToString("0") + "_1";
        var _TransformTank_1 = this.OABPart.PartTransform.FindChildRecursive(_TankNode);
        if (_TransformTank_1 != null)
        {
            // height 1 to slider value
            for (int cpt = 1; cpt < (int)ScaleH; cpt++)
            {
                string _newname = "Container_" + modele.ToString("0") + "_" + (cpt + 1).ToString("0");
                var _PartToCopy = this.OABPart.PartTransform.FindChildRecursive(_namecopy).gameObject;
                // --------------------------------
                if ((_PartToCopy != null) && (this.OABPart.PartTransform.FindChildRecursive(_newname) == null))
                {
                    // Container
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
                        float newbottomz = -(((cpt + 1) * Settings.ScalingCont[modele]) + Settings.ScalingTop[modele]);
                        _PartBottom.localPosition = new Vector3(0f, 0f, newbottomz);
                    }
                    // adjust vessel Attach nodes
                    //OnOABAdjustNodeAttach((float)ScaleWidth, modele);
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
                        float newbottomz = -(((cpt - 1) * Settings.ScalingCont[modele]) + Settings.ScalingTop[modele]);
                        _PartBottom.localPosition = new Vector3(0f, 0f, newbottomz);
                    }
                    // adjust vessel Attach nodes
                    //OnOABAdjustNodeAttach((float)ScaleWidth, modele);
                }
            }
            // Adjust possition to node bottom and connected parts
            K.Log("OnOABScaleHeightNode:" + ScaleH);
            OnOABScaleHeightNode(ScaleH, modele);
            // Adjust collider size
            var _ColliderToScale = this.OABPart.PartTransform.FindChildRecursive(_namecollider).gameObject;
            if (_ColliderToScale != null)
            {
                _ColliderToScale.transform.localScale = new Vector3(1f, ScaleH, 1f);
            }
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
    private void OnOABScaleWidthChanged(float ScaleW)
    {
        // Width scale of (all) Tanks parts
        OnOABScaleWPart(this.OABPart.PartTransform.FindChildRecursive("AllTanks"), (int)ScaleW);
        OnOABScaleWidthtNode(ScaleW, this.Model);
        // update tank mass
        MassModifier((int)ScaleW, ScaleHeight, Model, idresource);
        // replace nodes (it depends of tank width & height)
//OnOABAdjustNodeAttach(ScaleW, Model);
//OnOabAdjustSurface();
        // update vessel information for engineer report
        UpdateVesselInfo();
        // Update Tank module
        RefreshTank();
    }
    // Slider part Height change -> action
    private void OnOABScaleHeightChanged(float ScaleH)
    {
        // create new tank container part -> height change
        OnOABScaleHPart(ScaleH, Model);
        // update tank mass
        MassModifier(ScaleWidth, (int)ScaleH, Model, idresource);
        // update vessel information for engineer report
        UpdateVesselInfo();
        // Update Tank module
        RefreshTank();
    }
    public void OnOABScaleWidthtNode(float ScaleW, int modele)
    {
        float TotalCont = this.ScaleHeight * Settings.ScalingCont[modele];
        float newy = -((2 * Settings.ScalingTop[modele]) + TotalCont) * Settings.Scaling[(int)ScaleW];
        Vector3 LocalTransformation = new Vector3(0f, newy, 0f);
        this._floatingNodeB = this.OABPart.FindNodeWithTag("bottom");
        if (this._floatingNodeB != null)
        {
            _floatingNodeB.NodeTransform.localPosition = LocalTransformation;
        }
    }

    private void OnOABScaleHeightNode(float ScaleH, int modele)
    {
        this._floatingNodeB = this.OABPart.FindNodeWithTag("bottom");
        float newy = -(ScaleH - this.OldScaleHeight) * Settings.ScalingCont[modele] * Settings.Scaling[(int)this.ScaleWidth];
        Vector3 LocalTransformation = new Vector3(0f, newy, 0f);
        _floatingNodeB.NodeTransform.localPosition += LocalTransformation;
        foreach (IObjectAssemblyPart AttachedPart in this.OABPart.Children)
        {
            Quaternion TankRot = this.OABPart.PartTransform.rotation;
            AttachedPart.PartTransform.localPosition += TankRot * LocalTransformation;
        }

        /*
        float TotalCont = ScaleH * Settings.ScalingCont[modele];
        float newy = -((2 * Settings.ScalingTop[modele]) + TotalCont) * Settings.Scaling[(int)this.ScaleWidth];
        Vector3 LocalTransformation = new Vector3(0f, newy, 0f);

        this.OABPart.SetNodeLocalPosition(this._floatingNodeB, LocalTransformation);


        /*
        Vector3 DeltaLocalPos;
        if (ScaleH > this.OldScaleHeight) { DeltaLocalPos = LocalTransformation - _floatingNodeB.NodeTransform.localPosition; }
        else { DeltaLocalPos = _floatingNodeB.NodeTransform.localPosition - LocalTransformation; }
        if (this._floatingNodeB != null)
        {
            _floatingNodeB.NodeTransform.localPosition = LocalTransformation;
            Quaternion TankRot = this.OABPart.PartTransform.rotation;
            foreach (IObjectAssemblyPart AttachedPart in this.OABPart.Children)
            {
                //AttachedPart.PartTransform.localPosition += TankRot * DeltaLocalPos;
                //AttachedPart.SetNodeLocalPosition(AttachedPart.MyNodeConnectedToParent, TankRot * LocalTransformation);
            }

            // Adjust position of Bottom Node
            //this.OABPart.SetNodeLocalPosition(this._floatingNodeB, LocalTransformation);
            //if (this.OldScaleWidth < ScaleH) { _floatingNodeB.NodeTransform.localPosition = LocalTransformation - _floatingNodeB.NodeTransform.localPosition; } 
            //else { _floatingNodeB.NodeTransform.localPosition = _floatingNodeB.NodeTransform.localPosition - LocalTransformation; }
            //foreach (IObjectAssemblyPart AttachedPart in this.OABPart.Children)
            //{
            //if (AttachedPart.MyNodeConnectedToParent != null)
            //{
            //AttachedPart.SetNodeLocalPosition(AttachedPart.MyNodeConnectedToParent, LocalTransformation);
            //AttachedPart.MyNodeConnectedToParent.NodeTransform.localPosition = LocalTransformation;
            //AttachedPart.PartTransform.localPosition = LocalTransformation;
            //}

                //}
        }
        */
    }
/*
private void OnOABSResourceChanged(string Resourcechoice)
{
    string ResourceLabel = Enum.GetName(typeof(FuelTypes), Int32.Parse(Resourcechoice));
    //K.Log("DEBUGLOG OnOABSResourceChanged :"+ Resourcechoice + " : " + ResourceLabel);
    this.idresource = Int32.Parse(Resourcechoice);
}
*/
    public void OnFlyCreateContainer(float ScaleH, int modele)
    {
        // using tank model choice (int modele) to catch gameobject part for rebuild tank
        string _TankNode = "Tank_" + modele.ToString("0");
        string _namecopy = "Container_" + modele.ToString("0") + "_1";
        string _namecolcopy = "col_" + modele.ToString("0") + "_1";
        string _namebottom = "Bottom_" + modele.ToString("0");
        var _TransformTank_1 = this.part.FindModelTransform(_TankNode);
        if (_TransformTank_1 != null)
        {
            for (int cpt = 1; cpt < (int)ScaleH; cpt++)
            {
                string _newname = "Container_" + modele.ToString("0") + "_" + (cpt + 1).ToString("0");
                string _newcolname = "col_" + modele.ToString("0") + "_" + (cpt + 1).ToString("0");
                var _PartToCopy = this.part.FindModelTransform(_namecopy).gameObject;
                var _ColToCopy = this.part.FindModelTransform(_namecolcopy).gameObject;
                // --------------------------------
                if ((_PartToCopy != null) && (this.part.FindModelTransform(_newname) == null))
                {
                    // Container
                    GameObject gameObjnew = UnityEngine.Object.Instantiate<GameObject>(_PartToCopy);
                    gameObjnew.name = _newname;
                    gameObjnew.transform.parent = _TransformTank_1;
                    float newz = -(cpt * Settings.ScalingCont[modele]);
                    gameObjnew.transform.localPosition = new Vector3(0f, 0f, newz);
                    gameObjnew.transform.localRotation = _PartToCopy.transform.localRotation;
                    gameObjnew.transform.localScale = _PartToCopy.transform.localScale;
                    // Collider
                    GameObject gameColnew = UnityEngine.Object.Instantiate<GameObject>(_ColToCopy);
                    gameColnew.name = _newcolname;
                    gameColnew.transform.parent = _TransformTank_1;
                    gameColnew.transform.localPosition = new Vector3(0f, 0f, newz);
                    gameColnew.transform.localRotation = _ColToCopy.transform.localRotation;
                    gameColnew.transform.localScale = _ColToCopy.transform.localScale;
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

    // Adjust SurfaceNode connect after Wscale
    public void OnOabAdjustSurface()
    {
        _floatingNodeS = this.OABPart.FindNodeWithTag("srfAttach");
        if (_floatingNodeS.IsConnected)
        {
            IObjectAssemblyPartNode ConnecPartTNode = _floatingNodeS.ConnectedPart.FindNodeAttachedPart(this.PartIGGuid);
            if (ConnecPartTNode != null)
            {
                _floatingNodeS.NodeTransform.localPosition = ConnecPartTNode.NodeTransform.localPosition;
            }
                
        }
    }
    
    // calculate AttachNode with Scale in OAB
    public void OnOABAdjustNodeAttach(float scalewidth, int modele)
    {
        this._floatingNodeT = this.OABPart.FindNodeWithTag("top");
        this._floatingNodeB = this.OABPart.FindNodeWithTag("bottom");
        this._floatingNodeS = this.OABPart.FindNodeWithTag("srfAttach");
        // Bottom AttachNode
        if (this._floatingNodeB != null)
        {
            float TotalCont = (float)ScaleHeight * Settings.ScalingCont[modele];
            float newy = -((2 * Settings.ScalingTop[modele]) + TotalCont) * Settings.Scaling[(int)scalewidth];
            Vector3 NodeBlocalpos = new Vector3(0f, newy, 0f);
            this.OABPart.SetNodeLocalPosition(this._floatingNodeB, NodeBlocalpos);
            if (this._floatingNodeT != null)
            {
                float yTop = _floatingNodeT.AssemblyRelativePosition.y;
                _floatingNodeB.AssemblyRelativePosition.Set(0f, yTop + newy, 0f);
                // --------------------
                if (this._floatingNodeB.IsConnected)
                {
                    IObjectAssemblyPart ConnectedPartB = this._floatingNodeB.ConnectedPart as IObjectAssemblyPart;
                    if (ConnectedPartB != null)
                    {
                        IGGuid IDC = (this._floatingNodeB.ConnectedPart as ObjectAssemblyPart).GlobalId;
                        if (IDC != null)
                        {
                            IObjectAssemblyPartNode ConnecPNode = ConnectedPartB.FindNodeAttachedPart(this.PartIGGuid);
                            ConnecPNode.AssemblyRelativePosition.Set(0f, yTop + newy, 0f);
                        }
                    }
                }
            }
        }
        // Surface AttachNode
        if (this._floatingNodeS != null)
        {
            float newy = -(((2 * Settings.ScalingTop[modele]) + Settings.ScalingCont[modele]) * 0.5f * Settings.Scaling[(int)scalewidth]);
            float newz = Settings.ScalingRad[modele] * Settings.Scaling[(int)scalewidth];
            Vector3 NodeSlocalpos = new Vector3(0f, newy, newz);
            this.OABPart.SetNodeLocalPosition(this._floatingNodeS, NodeSlocalpos);
        }
        // chlidren parts
        foreach (ObjectAssemblyPart ConnectedPart in this.OABPart.Children)
        {
            if (ConnectedPart != null)
            {
                Vector3 NodeSlocalpos = new Vector3(ConnectedPart.transform.localPosition.x * Settings.Scaling[modele], ConnectedPart.transform.localPosition.y * Settings.Scaling[modele], ConnectedPart.transform.localPosition.z);
            }
        }
    }

    public void AdjustFinalPart()
    {
        // ------------------------------------------------
        if (this._floatingNodeB == null)  { this._floatingNodeB = this.OABPart.FindNodeWithTag("bottom"); }
        if (this._floatingNodeB != null)
        {
            // Tank OriginalPartLocalAttachPosition
            this.OABPart.OriginalPartLocalAttachPosition = this.OABPart.OriginalNodeLocalAttachPosition;
            // Connected bottom part
            if (this._floatingNodeB.IsConnected)
            {
                IObjectAssemblyPart ConnectedBPart = this._floatingNodeB.ConnectedPart;
                if (ConnectedBPart != null)
                {
                    IObjectAssemblyPartNode ConnecPartTNode = ConnectedBPart.FindNodeAttachedPart(this.PartIGGuid);
                    if (ConnecPartTNode != null)
                    {
                        ConnectedBPart.AssemblyRelativePosition = _floatingNodeB.AssemblyRelativePosition;
                        ConnectedBPart.OriginalPartLocalAttachPosition = ConnectedBPart.OriginalNodeLocalAttachPosition - ConnecPartTNode.PartRelativePosition;
                        ConnectedBPart.ParentNodeRelativePosition = ConnectedBPart.OriginalPartLocalAttachPosition - ConnectedBPart.OriginalNodeLocalAttachPosition;
                        ConnectedBPart.ParentPartRelativePosition = _floatingNodeB.PartRelativePosition + ConnectedBPart.ParentNodeRelativePosition;
                        (ConnecPartTNode as ObjectAssemblyPartNode).PartRelativePosition = _floatingNodeB.PartRelativePosition;
                    }
                }
            }
        }
    }

    public void AjustSymetricPart()
    {
        IObjectAssemblyPart ConnectedSPart = this._floatingNodeS.ConnectedPart;
        if (ConnectedSPart != null)
        {
            this.OABPart.AssemblyRelativePosition = this._data_SizerTank.AssemblyRelativePosition;
        }
    }

    public void MassModifier(int wscale, int hscale, int modele, int id_ressource)
    {
        this._panelMass = (2 * Settings.GetMassT(modele,wscale)); 
        for (int i=1; i<=ScaleHeight; i++)
        {
            this._panelMass += Settings.GetMassC(modele, wscale);
        }
        this._data_SizerTank.DryMass = this._panelMass;
        this._data_SizerTank.mass = this._panelMass;

        ResourceCapacityModifier(wscale, hscale, modele, id_ressource);

        this.OABPart.AvailablePart.PartData.mass = this._panelMass;
        (this.OABPart as ObjectAssemblyPart).mass = this._panelMass;
        (this.OABPart as ObjectAssemblyPart).UpdateMassValues();
    }

    public void ResourceCapacityModifier(int wscale, int hscale, int modele, int id_ressource)
    {
        // Calculate Tank volume
        this.Resourcevolume = Settings.GetVolT(modele, wscale) * 2;
        for (int i=1;i<= hscale; i++)
        {
            this.Resourcevolume += Settings.GetVolC(modele, wscale);
        }
        // Id Tank Resource 
        ResourceDefinitionID resourceIdFromName = this.Game.ResourceDefinitionDatabase.GetResourceIDFromName(Enum.GetName(typeof(FuelTypes), id_ressource));
        this.definitionData = GameManager.Instance.Game.ResourceDefinitionDatabase.GetDefinitionData(resourceIdFromName);

        if (this.PartBackingMode == PartBehaviourModule.PartBackingModes.OAB)
        {
            // modify container volume
            IResourceContainer container = this.OABPart.Containers[0];
            IPartModule IPM = this.OABPart.Modules.ValuesList[2];
            var modifs = new List<double>();
            modifs.Add(this.Resourcevolume * 0.2);
            modifs.Add(this.Resourcevolume * 0.8);
            // methane
            ResourceDefinitionID id1 = this.Game.ResourceDefinitionDatabase.GetResourceIDFromName("methane");
            int cpt1 = (container as ResourceContainer).GetDataIndexFromID(id1);
            (container as ResourceContainer).InternalModifyData(id1, this.Resourcevolume * 0.2, true, false, this.Resourcevolume * 0.2);
            // oxyder
            ResourceDefinitionID id2 = this.Game.ResourceDefinitionDatabase.GetResourceIDFromName("oxidizer");
            int cpt2 = (container as ResourceContainer).GetDataIndexFromID(id1);
            (container as ResourceContainer).InternalModifyData(id2, this.Resourcevolume * 0.8, true, false, this.Resourcevolume * 0.8);
            // modify 
            this.OABPart.AvailablePart.PartData.resourceContainers[0].name = Enum.GetName(typeof(FuelTypes), id_ressource);
            this.OABPart.AvailablePart.PartData.resourceContainers[0].capacityUnits = this.Resourcevolume;
            this.OABPart.AvailablePart.PartData.resourceContainers[0].initialUnits = this.Resourcevolume;
            this.OABPart.AvailablePart.PartData.resourceContainers[0].NonStageable = false;

            (this.OABPart.Resources[0] as ObjectAssemblyResource).Name = Enum.GetName(typeof(FuelTypes), id_ressource);
            (this.OABPart.Resources[0] as ObjectAssemblyResource).Capacity = this.Resourcevolume;
            (this.OABPart.Resources[0] as ObjectAssemblyResource).Count = this.Resourcevolume;
        }
    }

    public void Freeze(bool action)
    {
        K.Log("");
        // freeze / unfreeze resources (unused)
        (this.OABPart.Containers[0] as ResourceContainer)._resourceDefsFrozen = action;
        GameManager.Instance.Game.ResourceDefinitionDatabase._isDefinitionDataFrozen = action;
        K.Log("");
    }

    public void RefreshTank()
    {
        // refresh module
        this.OABPart.TryGetModule(typeof(Module_ResourceCapacities), out var module);
        module.OnShutdown();
        (module as Module_ResourceCapacities)._valueChangeHandlers.Clear();
        (module as Module_ResourceCapacities).dataResourceCapacities.RebuildDataContext();
        module.OnInitialize();
        // Refresh PAM windows
        if (GameManager.Instance.Game.PartsManager.IsVisible)
        {
            if (Game.OAB.Current.Game.PartsManager.PartsList._allParts.ContainsKey(this.PartIGGuid))
                Game.OAB.Current.Game.PartsManager.PartsList.ScrollToPart(this.PartIGGuid);
        }
    }

    public override void OnModuleOABUpdate(float deltaTime)
    {
        // update part localposition if not directly coming back from fly
        _data_SizerTank.AssemblyRelativePosition = this.OABPart.AssemblyRelativePosition;
    }

    public override void OnShutdown()
    {

    }
}
