using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DeCrawl.Primitives;
using DeCrawl.Utils;

namespace DeCrawl.Systems
{

    public class PhaseRecorder : FindingSingleton<PhaseRecorder>, StateSaver
    {
        private new void Awake()
        {
            base.Awake();    

            if (instance == this)
            {
                foreach (var phased in InterfaceFinder.FindMonoBeahviourWithIPhased())
                {
                    phased.OnPhaseChange += Phased_OnPhaseChange;
                }
            }
        }

        private new void OnDestroy()
        {
            base.OnDestroy();

            foreach (var phased in InterfaceFinder.FindMonoBeahviourWithIPhased())
            {
                phased.OnPhaseChange -= Phased_OnPhaseChange;
            }
        }

        Dictionary<string, string> phases = new Dictionary<string, string>();

        private void Phased_OnPhaseChange(string id, string phase)
        {
            phases[id] = phase;
            Debug.Log($"Phased changed: {phase} ({id})");
        }

        public void DeserializeState(string json)
        {            
            var dto = JsonUtility.FromJson<Dto>(json);
            
            // Restore own cache
            phases.Clear();
            foreach (var record in dto.PhaseRecords)
            {
                phases[record.Id] = record.Phase;
            }

            // Update all known components
            foreach (var phased in InterfaceFinder.FindMonoBeahviourWithIPhased())
            {
                if (phases.ContainsKey(phased.Id))
                {
                    phased.RestorePhase(phases[phased.Id]);
                }
            }
        }

        [System.Serializable]
        private struct PhaseRecord
        {
            public string Id;
            public string Phase;

            public PhaseRecord(string id, string phase)
            {
                Id = id;
                Phase = phase;
            }
        }

        [System.Serializable]
        private struct Dto
        {
            public PhaseRecord[] PhaseRecords;

            public Dto(Dictionary<string, string> phases)
            {
                PhaseRecords = phases.Keys.Select(k => new PhaseRecord(k, phases[k])).ToArray();
            }
        }

        public string SerializeState()
        {
            var dto = new Dto(phases);
            return JsonUtility.ToJson(dto);
        }
    }
}