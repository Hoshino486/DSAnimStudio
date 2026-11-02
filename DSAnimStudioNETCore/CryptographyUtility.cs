/*
The MIT License (MIT)

Copyright(c) 2015 Atvaark

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.IO;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

namespace DSAnimStudio
{
    public static class CryptographyUtility
    {
        public static byte[] DecryptAesEcb(Stream inputStream, byte[] key)
        {
            var cipher = CreateAesEcbCipher(key);
            return DecryptAes(inputStream, cipher, inputStream.Length);
        }

        public static byte[] DecryptAesCbc(Stream inputStream, byte[] key, byte[] iv)
        {
            AesEngine engine = new AesEngine();
            KeyParameter keyParameter = new KeyParameter(key);
            ICipherParameters parameters = new ParametersWithIV(keyParameter, iv);

            BufferedBlockCipher cipher = new BufferedBlockCipher(new CbcBlockCipher(engine));
            cipher.Init(false, parameters);
            return DecryptAes(inputStream, cipher, inputStream.Length);
        }

        public static byte[] DecryptAesCtr(Stream inputStream, byte[] key, byte[] iv)
        {
            AesEngine engine = new AesEngine();
            KeyParameter keyParameter = new KeyParameter(key);
            ICipherParameters parameters = new ParametersWithIV(keyParameter, iv);

            BufferedBlockCipher cipher = new BufferedBlockCipher(new SicBlockCipher(engine));
            cipher.Init(false, parameters);
            return DecryptAes(inputStream, cipher, inputStream.Length);
        }

        public static byte[] EncryptAesCtr(byte[] input, byte[] key, byte[] iv)
        {
            IBufferedCipher cipher = CipherUtilities.GetCipher("AES/CTR/NoPadding");
            cipher.Init(true, new ParametersWithIV(ParameterUtilities.CreateKeyParameter("AES", key), iv));
            return cipher.DoFinal(input);
        }

        private static BufferedBlockCipher CreateAesEcbCipher(byte[] key)
        {
            AesEngine engine = new AesEngine();
            KeyParameter parameter = new KeyParameter(key);
            BufferedBlockCipher cipher = new BufferedBlockCipher(engine);
            cipher.Init(false, parameter);
            return cipher;
        }

        private static byte[] DecryptAes(Stream inputStream, BufferedBlockCipher cipher, long length)
        {
            int blockSize = cipher.GetBlockSize();
            int inputLength = (int)length;
            int paddedLength = inputLength;
            if (paddedLength % blockSize > 0)
            {
                paddedLength += blockSize - paddedLength % blockSize;
            }

            byte[] input = new byte[paddedLength];
            byte[] output = new byte[cipher.GetOutputSize(paddedLength)];

            inputStream.Read(input, 0, inputLength);
            int len = cipher.ProcessBytes(input, 0, input.Length, output, 0);
            cipher.DoFinal(output, len);
            return output;
        }

        /// <summary>
        ///     使用给定的解密密钥解密文件。
        /// </summary>
        /// <param name="filePath">一个加密的文件</param>
        /// <param name="key">PEM 格式的 RSA 密钥</param>
        /// <exception cref="ArgumentNullException">当参数 filePath 为 null 时抛出</exception>
        /// <exception cref="ArgumentNullException">当参数 keyPath 为 null 时抛出</exception>
        /// <returns>包含解密后文件内容的内存流</returns>
        public static MemoryStream DecryptRsa(string filePath, string key)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            AsymmetricKeyParameter keyParameter = GetKeyOrDefault(key);
            RsaEngine engine = new RsaEngine();
            engine.Init(false, keyParameter);

            MemoryStream outputStream = new MemoryStream();
            using (FileStream inputStream = File.OpenRead(filePath))
            {

                int inputBlockSize = engine.GetInputBlockSize();
                int outputBlockSize = engine.GetOutputBlockSize();
                byte[] inputBlock = new byte[inputBlockSize];
                while (inputStream.Read(inputBlock, 0, inputBlock.Length) > 0)
                {
                    byte[] outputBlock = engine.ProcessBlock(inputBlock, 0, inputBlockSize);

                    int requiredPadding = outputBlockSize - outputBlock.Length;
                    if (requiredPadding > 0)
                    {
                        byte[] paddedOutputBlock = new byte[outputBlockSize];
                        outputBlock.CopyTo(paddedOutputBlock, requiredPadding);
                        outputBlock = paddedOutputBlock;
                    }

                    outputStream.Write(outputBlock, 0, outputBlock.Length);
                }
            }

            outputStream.Seek(0, SeekOrigin.Begin);
            return outputStream;
        }

        // <summary>
        ///     使用给定的解密密钥解密文件。
        /// </summary>
        /// <param name="inputStream">一个加密的文件</param>
        /// <param name="key">PEM 格式的 RSA 密钥</param>
        /// <exception cref="ArgumentNullException">当参数 filePath 为 null 时抛出</exception>
        /// <exception cref="ArgumentNullException">当参数 keyPath 为 null 时抛出</exception>
        /// <returns>包含解密后文件内容的内存流</returns>
        public static MemoryStream DecryptRsa(Stream inputStream, string key)
        {
            if (inputStream == null)
            {
                throw new ArgumentNullException(nameof(inputStream));
            }

            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            AsymmetricKeyParameter keyParameter = GetKeyOrDefault(key);
            RsaEngine engine = new RsaEngine();
            engine.Init(false, keyParameter);

            MemoryStream outputStream = new MemoryStream();

            int inputBlockSize = engine.GetInputBlockSize();
            int outputBlockSize = engine.GetOutputBlockSize();
            byte[] inputBlock = new byte[inputBlockSize];
            while (inputStream.Read(inputBlock, 0, inputBlock.Length) > 0)
            {
                byte[] outputBlock = engine.ProcessBlock(inputBlock, 0, inputBlockSize);

                int requiredPadding = outputBlockSize - outputBlock.Length;
                if (requiredPadding > 0)
                {
                    byte[] paddedOutputBlock = new byte[outputBlockSize];
                    outputBlock.CopyTo(paddedOutputBlock, requiredPadding);
                    outputBlock = paddedOutputBlock;
                }

                outputStream.Write(outputBlock, 0, outputBlock.Length);
            }

            outputStream.Seek(0, SeekOrigin.Begin);
            return outputStream;
        }

        public static AsymmetricKeyParameter GetKeyOrDefault(string key)
        {
            try
            {
                PemReader pemReader = new PemReader(new StringReader(key));
                return (AsymmetricKeyParameter)pemReader.ReadObject();
            }
            catch
            {
                return null;
            }
        }
    }
}