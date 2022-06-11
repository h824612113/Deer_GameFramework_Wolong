﻿using Deer;
using GameFramework.Resource;
using System;
using System.Collections.Generic;
using System.Reflection;
using UGFExtensions.SpriteCollection;
using UGFExtensions.Texture;
using UGFExtensions.Timer;
using UnityEngine;
using UnityGameFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;
/// <summary>
/// 游戏入口。
/// </summary>
public partial class GameEntry
{
    /// <summary>
    /// 获取游戏设置组件
    /// </summary>
    public static GameSettingsComponent GameSettings => _gameSettings ??= UnityGameFramework.Runtime.GameEntry.GetComponent<GameSettingsComponent>();
    private static GameSettingsComponent _gameSettings;

    public static MessengerComponent Messenger => _messenger ??= UnityGameFramework.Runtime.GameEntry.GetComponent<MessengerComponent>();
    private static MessengerComponent _messenger;

    public static CameraComponent Camera => _camera ??= UnityGameFramework.Runtime.GameEntry.GetComponent<CameraComponent>();
    private static CameraComponent _camera;

    public static NetConnectorComponent NetConnector => _netConnector ??= UnityGameFramework.Runtime.GameEntry.GetComponent<NetConnectorComponent>();
    private static NetConnectorComponent _netConnector;

    public static ConfigComponent Config => _config ??= UnityGameFramework.Runtime.GameEntry.GetComponent<ConfigComponent>();
    private static ConfigComponent _config;

    public static MainThreadDispatcherComponent MainThreadDispatcher => _mainThreadDispatcher ??= UnityGameFramework.Runtime.GameEntry.GetComponent<MainThreadDispatcherComponent>();
    private static MainThreadDispatcherComponent _mainThreadDispatcher;

    public static TextureSetComponent TextureSet => _textureSet ??= UnityGameFramework.Runtime.GameEntry.GetComponent<TextureSetComponent>();
    private static TextureSetComponent _textureSet;

    public static SpriteCollectionComponent SpriteCollection => _spriteCollection ??= UnityGameFramework.Runtime.GameEntry.GetComponent<SpriteCollectionComponent>();
    private static SpriteCollectionComponent _spriteCollection;

    public static TimerComponent Timer => _timer ??= UnityGameFramework.Runtime.GameEntry.GetComponent<TimerComponent>();
    private static TimerComponent _timer;

    public static AssetObjectComponent AssetObject => _assetObject ??= UnityGameFramework.Runtime.GameEntry.GetComponent<AssetObjectComponent>();
    private static AssetObjectComponent _assetObject;


    private static void InitCustomDebuggers()
    {
        // 将来在这里注册自定义的调试器
        GMNetWindow netWindow = new GMNetWindow();
        Debugger.SetGMNetWindowHelper(netWindow);

        CustomSettingsWindow customSettingWindow = new CustomSettingsWindow();
        Debugger.SetCustomSettingWindowHelper(customSettingWindow);
    }
    /// <summary>
    /// 初始化组件一些设置
    /// </summary>
    private static void InitComponentsSet()
    {

    }
    /// <summary>
    /// 加载自定义组件
    /// </summary>
    private static void LoadCustomComponent() 
    {
        GameEntryMain.Resource.LoadAsset("Assets/Deer/AssetsHotfix/GF/Customs.prefab", new LoadAssetCallbacks(loadAssetSuccessCallback,loadAssetFailureCallback));
    }

    private static void loadAssetFailureCallback(string assetName, LoadResourceStatus status, string errorMessage, object userData)
    {
        
    }

    private static void loadAssetSuccessCallback(string assetName, object asset, float duration, object userData)
    {
        if (GameObject.Find("DeerGF/Customs")!= null)
        {
            Resource.UnloadAsset(asset);
            return;
        }
        GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)asset);
        gameObject.name = "Customs";
        gameObject.transform.parent = GameObject.Find("DeerGF").transform;
        ResetProcedure();
    }
    private static List<Assembly> m_HotfixAssemblys;
    private static ProcedureBase m_EntranceProcedureBase;
    private static string m_EntranceProcedureTypeName = "Deer.ProcedurePreload";
    private static void ResetProcedure() 
    {
        //卸载流程
        Fsm.DestroyFsm<GameFramework.Procedure.IProcedureManager>();
        GameFramework.Procedure.IProcedureManager procedureManager = GameFramework.GameFrameworkEntry.GetModule<GameFramework.Procedure.IProcedureManager>();
        //创建新的流程 HotFixFramework.Runtime
        var m_ProcedureTypeNames = TypeUtils.GetRuntimeTypeNames(typeof(ProcedureBase), m_HotfixAssemblys);
        ProcedureBase[] procedures = new ProcedureBase[m_ProcedureTypeNames.Length];
        for (int i = 0; i < m_ProcedureTypeNames.Length; i++)
        {
            Type procedureType = GameFramework.Utility.Assembly.GetType(m_ProcedureTypeNames[i]);
            if (procedureType == null)
            {
                Log.Error("Can not find procedure type '{0}'.", m_ProcedureTypeNames[i]);
                return;
            }

            procedures[i] = (ProcedureBase)Activator.CreateInstance(procedureType);
            if (procedures[i] == null)
            {
                Log.Error("Can not create procedure instance '{0}'.", m_ProcedureTypeNames[i]);
                return;
            }

            if (m_EntranceProcedureTypeName == m_ProcedureTypeNames[i])
            {
                m_EntranceProcedureBase = procedures[i];
            }
        }

        if (m_EntranceProcedureBase == null)
        {
            Log.Error("Entrance procedure is invalid.");
            return;
        }
        procedureManager.Initialize(GameFramework.GameFrameworkEntry.GetModule<GameFramework.Fsm.IFsmManager>(), procedures);
        procedureManager.StartProcedure(m_EntranceProcedureBase.GetType());
    }
    public static void Entrance(object[] objects) 
    {
        m_HotfixAssemblys = (List<Assembly>)objects[0];
        //初始化自定义调试器
        InitCustomDebuggers();
        InitComponentsSet();
        LoadCustomComponent();
    }
}
