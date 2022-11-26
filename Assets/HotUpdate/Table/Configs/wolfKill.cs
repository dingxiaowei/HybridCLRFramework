/*
 * auto generated by tools(注意:千万不要手动修改本文件)
 * wolfKill
 */
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Linq;

[Serializable]
public partial class wolfKill : IBinarySerializable
{
	/// <summary>
	/// 序号
	/// </summary>
	public int Id { get; set; }
	/// <summary>
	/// 人数
	/// </summary>
	public int PlayerCount { get; set; }
	/// <summary>
	/// 狼人
	/// </summary>
	public int Wolf { get; set; }
	/// <summary>
	/// 村民
	/// </summary>
	public int Villagers { get; set; }
	/// <summary>
	/// 预言家
	/// </summary>
	public int Prophet { get; set; }
	/// <summary>
	/// 女巫
	/// </summary>
	public int Witch { get; set; }
	/// <summary>
	/// 猎人
	/// </summary>
	public int Hunter { get; set; }
	/// <summary>
	/// 守卫
	/// </summary>
	public int Guard { get; set; }

	public void DeSerialize(BinaryReader reader)
	{
		Id = reader.ReadInt32();
		PlayerCount = reader.ReadInt32();
		Wolf = reader.ReadInt32();
		Villagers = reader.ReadInt32();
		Prophet = reader.ReadInt32();
		Witch = reader.ReadInt32();
		Hunter = reader.ReadInt32();
		Guard = reader.ReadInt32();
	}

	public void Serialize(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(PlayerCount);
		writer.Write(Wolf);
		writer.Write(Villagers);
		writer.Write(Prophet);
		writer.Write(Witch);
		writer.Write(Hunter);
		writer.Write(Guard);
	}
}

[Serializable]
public partial class wolfKillConfig : IBinarySerializable
{
	public List<wolfKill> wolfKillInfos = new List<wolfKill>();
	public void DeSerialize(BinaryReader reader)
	{
		int count = reader.ReadInt32();
		for (int i = 0;i < count; i++)
		{
			wolfKill tempData = new wolfKill();
			tempData.DeSerialize(reader);
			wolfKillInfos.Add(tempData);
		}
	}

	public void Serialize(BinaryWriter writer)
	{
		writer.Write(wolfKillInfos.Count);
		for (int i = 0; i < wolfKillInfos.Count; i++)
		{
			wolfKillInfos[i].Serialize(writer);
		}
	}

	public IEnumerable<wolfKill> QueryById(int id)
	{
		var datas = from d in wolfKillInfos
					where d.Id == id
					select d;
		return datas;
	}
}
