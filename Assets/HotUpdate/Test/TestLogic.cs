using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLogic : MonoBehaviour
{
    public class Skill
    {
        public int skillId;
        public string skillName;
    }
    
    public class Hero
    {
        public int heroId;
        public string heroName;
    }
    
    List<Skill> skillList = new List<Skill>();
    List<Hero> heroList = new List<Hero>();
    
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("TestLogic Start");
    }

    public void BuildData()
    {
        var skill1 = new Skill();
        skill1.skillName = "测试技能1";
        skill1.skillId = 1001;
        
        var skill2 = new Skill();
        skill2.skillName = "测试技能2";
        skill2.skillId = 1002;
        
        skillList.Add(skill1);
        skillList.Add(skill2);
        
        var hero = new Hero();
        hero.heroId = 1;
        hero.heroName = "赵云";
        heroList.Add(hero);
    }

    public void TestDebug()
    {
        BuildData();
        
        heroList.ForEach((o) =>
        {
            Debug.Log($"武将id:{o.heroId.ToString()} 武将名称:{o.heroName}");
        });
        
        skillList.ForEach((o) =>
        {
            Debug.Log($"技能id:{o.skillId.ToString()} 技能名称:{o.skillName}");
        });
    }
}
