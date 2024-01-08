using System;
using System.Security.Cryptography;
using System.Text;

namespace Connection.Utils {
public abstract class Crypto {
	private static readonly Aes Aes = Aes.Create();

	private static readonly RSA Rsa = RSA.Create();

	/**
     * 讀取伺服端傳送的 XML 格式 RSA 公鑰
     */
	public static void InitRsa(byte[] buffer) {
		Rsa.FromXmlString(Encoding.UTF8.GetString(buffer));
	}

	/**
     * 初始化 AES 設定並生成金耀。使用 RSA 加密金耀與 IV，並傳送給伺服端
     */
	public static void InitAes() {
		Aes.KeySize = 256; // 32 bytes * 8 = 256 bits
		Aes.BlockSize = 128; // 16 bytes * 8 = 128 bits
		Aes.Mode = CipherMode.CBC;
		Aes.Padding = PaddingMode.PKCS7;
		Aes.GenerateKey();
		Aes.GenerateIV();
	}

	public static byte[] GetAesKeyIv() {
		return Rsa.Encrypt(Encoding.UTF8.GetBytes( // 將字串轉為 bytes
				$"key: {Convert.ToBase64String(Aes.Key)}\r\n" +
				$"iv: {Convert.ToBase64String(Aes.IV)}")
			, RSAEncryptionPadding.OaepSHA1);
	}

	/**
     * AES 加密函數。加密輸入的參數，並返回 byte[]
     */
	public static byte[] EncryptWithAes(string data) {
		return EncryptWithAes(Encoding.UTF8.GetBytes(data));
	}

	public static byte[] EncryptWithAes(byte[] dataBytes) {
		using var encryptor = Aes.CreateEncryptor();
		return encryptor.TransformFinalBlock(dataBytes, 0, dataBytes.Length);
	}

	/**
     * 解密接收到的 AES 加密資料
     */
	public static byte[] DecryptWithAes(byte[] data, int length) {
		return Aes.CreateDecryptor().TransformFinalBlock(data, 0, length);
	}
}
}