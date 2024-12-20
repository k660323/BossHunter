using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class ObjectManager
{
    public UnityAction<int> OnSpawnEvent;
    public PlayerController Player { get; private set; }
    //public HashSet<MonsterController> Monsters { get; } = new HashSet<MonsterController>();
    //public HashSet<ProjectileController> Projectiles { get; } = new HashSet<ProjectileController>();
    //public HashSet<GemController> Gems { get; } = new HashSet<GemController>();
    
    public T Spawn<T>(Vector3 position, int templateID = 0, string prefabName = "") where T : BaseController
	{
		System.Type type = typeof(T);

        if (type == typeof(PlayerController))
        {
            // TODO : Data
            string name = "";

            switch(templateID)
            {
                case 0:
                    name = "Player";
                    break;
            }

        	GameObject go = Managers.Resource.Instantiate(name);
        	go.name = prefabName;
        	go.transform.position = position;

        	PlayerController pc = go.GetOrAddComponent<PlayerController>();
        	Player = pc;
        	pc.Init();

        	return pc as T;
        }
        //else if (type == typeof(MonsterController))
        //{
        //	string name = "";

        //	switch (templateID)
        //	{
        //		case Define.GOBLIN_ID:
        //			name = "Goblin_01";
        //			break;
        //		case Define.SNAKE_ID:
        //			name = "Snake_01";
        //			break;
        //		case Define.BOSS_ID:
        //			name = "Boss_01";
        //			break;
        //	};

        //	GameObject go = Managers.Resource.Instantiate(name + ".prefab", pooling: true);
        //	go.transform.position = position;

        //	MonsterController mc = go.GetOrAddComponent<MonsterController>();
        //	Monsters.Add(mc);
        //	mc.Init();

        //	return mc as T;
        //}
        //else if (type == typeof(GemController))
        //{
        //	GameObject go = Managers.Resource.Instantiate(Define.EXP_GEM_PREFAB, pooling: true);
        //	go.transform.position = position;

        //	GemController gc = go.GetOrAddComponent<GemController>();
        //	Gems.Add(gc);
        //	gc.Init();

        //	string key = UnityEngine.Random.Range(0, 2) == 0 ? "EXPGem_01.sprite" : "EXPGem_02.sprite";
        //	Sprite sprite = Managers.Resource.Load<Sprite>(key);
        //	go.GetComponent<SpriteRenderer>().sprite = sprite;

        //	// TEMP
        //	GameObject.Find("@Grid").GetComponent<GridController>().Add(go);

        //	return gc as T;
        //}
        //else if (type == typeof(ProjectileController))
        //{
        //	GameObject go = Managers.Resource.Instantiate("FireProjectile.prefab", pooling: true);
        //	go.transform.position = position;

        //	ProjectileController pc = go.GetOrAddComponent<ProjectileController>();
        //	Projectiles.Add(pc);
        //	pc.Init();

        //	return pc as T;
        //}
        //else if (typeof(T).IsSubclassOf(typeof(SkillBase)))
        //{
        //	if (Managers.Data.SkillDic.TryGetValue(templateID, out Data.SkillData skillData) == false)
        //	{
        //		Debug.LogError($"ObjectManager Spawn Skill Failed {templateID}");
        //		return null;
        //	}

        //	GameObject go = Managers.Resource.Instantiate(skillData.prefab, pooling: false);
        //	go.transform.position = position;

        //	T t = go.GetOrAddComponent<T>();
        //	t.Init();

        //	return t;
        //}

        return null;
	}

	public void Despawn<T>(T obj) where T : BaseController
	{
		if (obj.IsValid() == false)
		{
			return;
		}

		//System.Type type = typeof(T);

		//if (type == typeof(PlayerController))
		//{
		//	// ?
		//}
		//// obj is MonsterController
		//else if (type == typeof(MonsterController) || type.IsSubclassOf(typeof(MonsterController)))
		//{
		//	Monsters.Remove(obj as MonsterController);
		//	Managers.Resource.Destroy(obj.gameObject);
		//}
		//else if (type == typeof(GemController))
		//{
		//	Gems.Remove(obj as GemController);
		//	Managers.Resource.Destroy(obj.gameObject);

		//	// TEMP
		//	GameObject.Find("@Grid").GetComponent<GridController>().Remove(obj.gameObject);
		//}
		//else if(type == typeof(ProjectileController))
		//{
		//	Projectiles.Remove(obj as ProjectileController);
		//	Managers.Resource.Destroy(obj.gameObject);
		//}
	}

	public void DespawnAllMonsters()
	{
		//var monsters = Monsters.ToList();

		//foreach (var monster in monsters)
		//	Despawn<MonsterController>(monster);
	}
}
