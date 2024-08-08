using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Tiles.Actions.General.Interfaces;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.State.Structs;
using Unity.Profiling;
using Mona.SDK.Brains.Core.Control;
using UnityGLTF;
using System.IO;

namespace Mona.SDK.Brains.Tiles.Actions.General
{
    [Serializable]
    public class ExportGlbInstructionTile : InstructionTile, IInstructionTileWithPreload, IActionInstructionTile, IActionStateEndInstructionTile
    {
        public const string ID = "ExportGlb";
        public const string NAME = "Export Glb";
        public const string CATEGORY = "General";
        public override Type TileType => typeof(ExportGlbInstructionTile);

        [SerializeField] private MonaBrainExportType _target = MonaBrainExportType.ThisBodyOnly;
        [BrainPropertyEnum(true)] public MonaBrainExportType Target { get => _target; set => _target = value; }

        [SerializeField] private string _targetTag;
        [BrainPropertyShow(nameof(Target), (int)MonaBrainExportType.Tag)]
        [BrainPropertyMonaTag(true)] public string TargetTag { get => _targetTag; set => _targetTag = value; }

        [SerializeField] private string _bodyArray;
        [BrainPropertyShow(nameof(Target), (int)MonaBrainBroadcastType.MyBodyArray)]
        [BrainPropertyValue(typeof(IMonaVariablesBodyArrayValue), true)] public string BodyArray { get => _bodyArray; set => _bodyArray = value; }

        [SerializeField] private bool _includeAttached = false;
        [BrainPropertyShow(nameof(Target), (int)MonaBrainExportType.Tag)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainExportType.MessageSender)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainExportType.OnConditionTarget)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainExportType.OnSelectTarget)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainExportType.MySpawner)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainExportType.LastSpawnedByMe)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainExportType.MyPoolPreviouslySpawned)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainExportType.MyPoolNextSpawned)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainExportType.LastSkin)]

        [BrainProperty(false)] public bool IncludeAttached { get => _includeAttached; set => _includeAttached = value; }

        [SerializeField] private bool _includeChildren = false;
        [BrainProperty(false)] public bool IncludeChildren { get => _includeChildren; set => _includeChildren = value; }

        [SerializeField] private string _fileName;
        [SerializeField] private string _fileNameValueName;
        [BrainProperty] public string FileName { get => _fileName; set => _fileName = value; }

        [BrainPropertyValueName(nameof(FileName), typeof(IMonaVariablesStringValue))] public string FileNameValueName { get => _fileNameValueName; set => _fileNameValueName = value; }

        private IMonaBrain _brain;

        private bool SendToAllAttached
        {
            get
            {
                switch (_target)
                {
                    case MonaBrainExportType.Self:
                        return false;
                    case MonaBrainExportType.Parent:
                        return false;
                    case MonaBrainExportType.Children:
                        return false;
                    case MonaBrainExportType.ThisBodyOnly:
                        return false;
                    default:
                        return _includeAttached;
                }
            }
        }

        //static readonly ProfilerMarker _profilerDo = new ProfilerMarker($"MonaBrains.{nameof(ExportGlbInstructionTile)}.{nameof(Do)}");

        public ExportGlbInstructionTile() { }

        public void Preload(IMonaBrain brain)
        {
            _brain = brain;
        }

        public IMonaBody GetBodyToControl()
        {
            return _brain.Body;
        }

        public override InstructionTileResult Do()
        {
            //_profilerDo.Begin();
            if (!string.IsNullOrEmpty(_fileNameValueName))
                _fileName = _brain.Variables.GetString(_fileNameValueName);


            switch (_target)
            {
                case MonaBrainExportType.Tag:
                    ExportTags();
                    break;
                case MonaBrainExportType.Self:
                    ExportWholeEntity(_brain.Body);
                    break;
                case MonaBrainExportType.Children:
                    ExportChildren(_brain.Body);
                    break;
                case MonaBrainExportType.ThisBodyOnly:
                    ExportGlb(_brain.Body);
                    break;
                case MonaBrainExportType.MyBodyArray:
                    ExportBodyArray(_brain);
                    break;
                default:
                    IMonaBody targetBody = GetTarget();

                    if (targetBody == null)
                        break;

                    if (SendToAllAttached)
                        ExportWholeEntity(targetBody);
                    else
                        ExportGlb(targetBody);
                    break;
            }

            return Complete(InstructionTileResult.Success);
        }

        private void ExportBodyArray(IMonaBrain brain)
        {
            var bodyArray = brain.Variables.GetBodyArray(_bodyArray);

            for (var i = 0; i < bodyArray.Count; i++)
            {
                if (SendToAllAttached)
                    ExportWholeEntity(bodyArray[i]);
                else
                {
                    ExportGlb(bodyArray[i]);
                }
            }
        }

        private void ExportTags()
        {
            var tag = _targetTag;
            /*TODOif (_appendPlayerId)
            {
                tag = $"{tag}{_brain.Player.PlayerId.ToString("00")}";
                //Debug.Log($"{nameof(BroadcastMessageToTagInstructionTile)} {tag}");
            }*/

            var bodies = MonaBodyFactory.FindByTag(tag);
            if (bodies.Count == 0)
                bodies = MonaBodyFactory.FindByTag(tag);

            for (var i = 0; i < bodies.Count; i++)
            {
                var body = bodies[i];
                ExportGlb(body);
            }
        }

        private void ExportWholeEntity(IMonaBody body)
        {
            IMonaBody topBody = body;
            while (topBody.Parent != null)
                topBody = topBody.Parent;

            ExportGlb(topBody);
        }

        private void ExportChildren(IMonaBody body)
        {
            var children = body.Children();

            for (var i = 0; i < children.Count; i++)
            {
                if (children[i] == null || !children[i].GetActive())
                    continue;

                ExportGlb(children[i]);
            }
        }

        private void ExportGlb(IMonaBody target)
        {

            if (target == null) return;

            var settings = GLTFSettings.GetOrCreateSettings();
            var exportContext = new ExportContext(settings)
            {
                
            };

            GLTFSceneExporter exporter = new GLTFSceneExporter(target.Transform, exportContext);

            if (string.IsNullOrEmpty(_fileName))
                _fileName = target.Transform.name;

            if (_fileName.IndexOf(".glb") > -1)
                _fileName = _fileName.Replace(".glb", "");

            _fileName += ".glb";

#if UNITY_WEBGL && !UNITY_EDITOR
            var contents = exporter.SaveGLBToByteArray(_fileName);
            if (WebGLFileSaver.IsSavingSupported())
                WebGLFileSaver.SaveFile(contents, _fileName, "model/gltf-binary");
#else
            var downloadsPath = "";
    #if UNITY_ANDROID && !UNITY_EDITOR
				downloadsPath = "/storage/emulated/0/Download/";
    #elif UNITY_IOS && !UNITY_EDITOR
				downloadsPath = Path.Combine(Application.persistentDataPath, "..", "Documents");
    #else
                downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
    #endif

            exporter.SaveGLB(downloadsPath, _fileName);
#endif


        }

        private IMonaBody GetTarget()
        {
            switch (_target)
            {
                case MonaBrainExportType.Parent:
                    return _brain.Body.Parent;
                case MonaBrainExportType.MessageSender:
                    var brain = _brain.Variables.GetBrain(MonaBrainConstants.RESULT_SENDER);
                    if (brain != null)
                        return brain.Body;
                    break;
                case MonaBrainExportType.OnConditionTarget:
                    return _brain.Variables.GetBody(MonaBrainConstants.RESULT_TARGET);
                case MonaBrainExportType.OnSelectTarget:
                    return _brain.Variables.GetBody(MonaBrainConstants.RESULT_HIT_TARGET);
                case MonaBrainExportType.MySpawner:
                    return _brain.Body.Spawner;
                case MonaBrainExportType.LastSpawnedByMe:
                    return _brain.Variables.GetBody(MonaBrainConstants.RESULT_LAST_SPAWNED);
                case MonaBrainExportType.MyPoolPreviouslySpawned:
                    return _brain.Body.PoolBodyPrevious;
                case MonaBrainExportType.MyPoolNextSpawned:
                    return _brain.Body.PoolBodyNext;
            }
            return null;
        }
    }
}