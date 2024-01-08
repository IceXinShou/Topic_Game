namespace Connection.Utils {
	public enum ClientHeader : byte {
		UpdatePos = 0b0000_0001,
		SpawnPoint = 0b0000_0010,
		Suicide = 0b0000_0011,
		BlockClaim = 0b0000_0100,
		Kill = 0b0000_0101
	}

	public enum ServerHeader : byte {
		SpawnPoint = 0b0000_0000,
		ChunkData = 0b0000_0001,
		LoseBlock = 0b0000_0010,
		PlayerJoin = 0b0000_0011,
		Scoreboard = 0b0000_0100,
		Dead = 0b0000_0101,
		PlayersPos = 0b0000_0110
	}
}