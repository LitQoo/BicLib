using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Configuration;
using UnityEngine;

namespace BicUtil.Crypto
{
    public class AES256
    {
        public static string KEY = ""; 
        public static string IV = "1234567890123456"; 

        public static void SetKey(string _key){
            if(string.IsNullOrEmpty(KEY) == false && _key != KEY){
                Debug.LogError("BicUtil.Crypto.AES256 Key error " + _key + "!=" + KEY);
            }
            
            KEY = _key;
        }
        
        public static string Encrypt(string _text){
            AesManaged aesManaged = new AesManaged();
            aesManaged.Mode = CipherMode.CBC;
            aesManaged.Padding = PaddingMode.PKCS7;
            aesManaged.KeySize = 256;
            aesManaged.Key = System.Text.Encoding.UTF8.GetBytes(KEY);
            aesManaged.IV = System.Text.Encoding.UTF8.GetBytes(IV);
            ICryptoTransform transform = aesManaged.CreateEncryptor();
            byte[] plainText = System.Text.Encoding.UTF8.GetBytes(_text);
            byte[] encrypted = transform.TransformFinalBlock(plainText, 0, plainText.Length);
            string base64ed = Convert.ToBase64String(encrypted);

            return base64ed.Trim();
        }

        public static string Decrypt(string _base64Text){
            AesManaged aesManaged = new AesManaged();
            aesManaged.Mode = CipherMode.CBC;
            aesManaged.Padding = PaddingMode.PKCS7;
            aesManaged.KeySize = 256;
            aesManaged.Key = System.Text.Encoding.UTF8.GetBytes(KEY);
            aesManaged.IV = System.Text.Encoding.UTF8.GetBytes(IV);
            ICryptoTransform transform = aesManaged.CreateDecryptor();
            var encrypted = Convert.FromBase64String(_base64Text);
            var plainText = transform.TransformFinalBlock(encrypted, 0, encrypted.Length);
            string decrypted = System.Text.Encoding.UTF8.GetString(plainText);

            return decrypted.Trim();
        }
    }
}