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
using Castle.Core.Resource;
using System;
using System.Reflection;
using KSP.Modules;
using System.ComponentModel;
using KSP.Messages;
using KSP.Sim.impl;
using KSP.Utilities;
using UnityEngine.Video;

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

    // ---- stats engineer ---
    private OABSessionInformation _stats;
    // ---- part -------------
    protected CorePartData CorePartData;
    private float _deltaUniverseTime;
    private int OldModel = 1;
    private Material Material = null;
    private float _panelMass;
    // ---- nodes ------------
    private IObjectAssemblyPartNode _floatingNodeB;
    private IObjectAssemblyPartNode _floatingNodeS;
    // ---- resources --------_partsManagerCategoryPrefabPool
    public int idresource = 8;
    ResourceDefinitionID resourceId;
    ResourceDefinitionDatabase definitionDatabase => GameManager.Instance.Game.ResourceDefinitionDatabase;
    ResourceDefinitionData definitionData;
    
    public ResourceContainer Container = new KSP.Sim.ResourceSystem.ResourceContainer();
    private float Resourcevolume;
    public IEnumerable<ContainedResourceData> CRData;
    private Dictionary<IResourceContainer, int> ContainerIndex = new Dictionary<IResourceContainer, int>();
    // TEST
    public GameManager GMGR => GameManager.Instance;
    private ObjectAssemblyPart KOABPart;
    private PartsManagerCore KPam;

    private struct RessUnits { ResourceDefinitionID RDID; double units; };
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
// Set PAM object
this.KPam = Game.OAB.Current.Game.PartsManager;
            // Set ResourceId to Methalox ID
            this.resourceId = GameManager.Instance.Game.ResourceDefinitionDatabase.GetResourceIDFromName(Enum.GetName(typeof(FuelTypes), idresource));
            // OABSessionInformation for updating engineer report windows
            this._stats = Game.OAB.Current.ActivePartTracker.stats;
            // show PAM config 
            this._data_SizerTank.SetVisible((IModuleDataContext)this._data_SizerTank.SliderScaleWidth, true);
            this._data_SizerTank.SetVisible((IModuleDataContext)this._data_SizerTank.SliderScaleHeight, true);
            this._data_SizerTank.SetVisible((IModuleDataContext)this._data_SizerTank.ResourcesList, true);
            // scale Width Tank
            OnOABScaleWPart(this.OABPart.PartTransform.FindChildRecursive("AllTanks"), ScaleWidth);
            // update Tank mass
            MassModifier(ScaleWidth, ScaleHeight, Model, idresource);
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
        MassModifier((int)ScaleH, ScaleHeight, Model, idresource);
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
        MassModifier(ScaleWidth, (int)ScaleH, Model, idresource);
        // update vessel information for engineer report
        UpdateVesselInfo();
    }

    private void OnOABSResourceChanged(string Resourcechoice)
    {
        string ResourceLabel = Enum.GetName(typeof(FuelTypes), Int32.Parse(Resourcechoice));
        //K.Log("DEBUGLOG OnOABSResourceChanged :"+ Resourcechoice + " : " + ResourceLabel);
        this.idresource = Int32.Parse(Resourcechoice);
        // -------------------------------------------------------------
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
            (container as ResourceContainer).InternalModifyData(id1, this.Resourcevolume * 0.2, true, false);
            // oxyder
            ResourceDefinitionID id2 = this.Game.ResourceDefinitionDatabase.GetResourceIDFromName("oxidizer");
            int cpt2 = (container as ResourceContainer).GetDataIndexFromID(id1);
            (container as ResourceContainer).InternalModifyData(id2, this.Resourcevolume * 0.8, true, false);
            // modify 
            this.OABPart.AvailablePart.PartData.resourceContainers[0].name = Enum.GetName(typeof(FuelTypes), id_ressource);
            this.OABPart.AvailablePart.PartData.resourceContainers[0].capacityUnits = this.Resourcevolume;
            this.OABPart.AvailablePart.PartData.resourceContainers[0].initialUnits = this.Resourcevolume;
            this.OABPart.AvailablePart.PartData.resourceContainers[0].NonStageable = false;

            (this.OABPart.Resources[0] as ObjectAssemblyResource).Name = Enum.GetName(typeof(FuelTypes), id_ressource);
            (this.OABPart.Resources[0] as ObjectAssemblyResource).Capacity = this.Resourcevolume;
            (this.OABPart.Resources[0] as ObjectAssemblyResource).Count = this.Resourcevolume;

            // refresh module
            this.OABPart.TryGetModule(typeof(Module_ResourceCapacities), out var module);
            module.OnShutdown();
            (module as Module_ResourceCapacities)._valueChangeHandlers.Clear();
            (module as Module_ResourceCapacities).dataResourceCapacities.RebuildDataContext();
            module.OnInitialize();
            // Refresh PAM windows
            //Game.OAB.Current.Game.PartsManager._partsList.PerformUpdate();
        }
        else
        {

        }
    }

    public void Freeze(bool action)
    {
        // freeze / unfreeze resources (unused)
        (this.OABPart.Containers[0] as ResourceContainer)._resourceDefsFrozen = action;
        GameManager.Instance.Game.ResourceDefinitionDatabase._isDefinitionDataFrozen = action;
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

    }
}
