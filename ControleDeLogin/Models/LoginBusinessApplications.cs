using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Security.Cryptography;

using IntuitiveEstruturas;
using Persistencia;

namespace ControleDeLogin.Models
{
    public static class LoginBusinessApplications
    {
        private static byte[] getAesKey()
        {
            return new byte[32] { 245, 112, 45, 68, 75, 44, 0, 12, 8, 55, 254, 1, 6, 9, 63, 12, 45, 11, 23, 25, 88, 17, 35, 52, 55, 45, 45, 45, 1, 2, 69, 210 };
        }

        private static byte[] getAesIV()
        {
            return new byte[16] { 114, 234, 201, 1, 3, 2, 54, 12, 45, 65, 2, 47, 8, 7, 69, 36 };
        }

        public static bool AdicionarDataPadraoCadastroUsuario(ref Usuarios objUsuario)
        {
            try
            {                
                objUsuario.DataCriacao = System.DateTime.Now.Date;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsTamanhoSenhaValido(string senhaNaoFormatada)
        {
            return senhaNaoFormatada.Length >= 8;
        }

        public static bool IsSenhaIgualLogin(ref Usuarios objUsuario)
        {
            return objUsuario.Senha == objUsuario.Login;
        }

        public static string getSenhaCriptografada(string senhaSemCriptografia)
        {
            Byte[] hashBytes;
            String hashHexadecimal;
            MD5 md5 = MD5.Create();

            hashBytes = md5.ComputeHash(Encoding.Unicode.GetBytes(senhaSemCriptografia));
            hashHexadecimal = BitConverter.ToString(hashBytes);
            hashHexadecimal = hashHexadecimal.Replace("-", String.Empty);

            return hashHexadecimal;
        }

        public static bool criptografarSenha(ref Usuarios objUsuario)
        {
            try
            {
                objUsuario.Senha = getSenhaCriptografada(objUsuario.Senha);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool adicionarGrupoAoUsuario(ref Usuarios objUsuario, int grupoId, int idEstab)
        {
            try
            {
                int idUsuario = objUsuario.Id;

                if (bdContext<ControleDeLoginEntities>.Instance.BD.GrupoUsuarios.Where(x => x.IdGrupo == grupoId && 
                                                                                            x.IdUsuario == idUsuario &&
                                                                                            x.IdEstabelecimento == idEstab).
                                                                                 Count() <= 0)
                {
                    GrupoUsuarios objGrupoUsuario = new GrupoUsuarios();
                    objGrupoUsuario.IdGrupo = grupoId;
                    objGrupoUsuario.IdEstabelecimento = idEstab;
                    objUsuario.GrupoUsuarios.Add(objGrupoUsuario);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool removerGrupoDoUsuario(ref Usuarios objUsuario, int grupoId, int idEstab)
        {
            try
            {
                int idUsuario = objUsuario.Id;
                GrupoUsuarios objGrupoUsuario = bdContext<ControleDeLoginEntities>.Instance.BD.GrupoUsuarios.Where(x => x.IdUsuario == idUsuario && 
                                                                                                                        x.IdGrupo == grupoId &&
                                                                                                                        x.IdEstabelecimento == idEstab).
                                                                                                             FirstOrDefault();

                if (objGrupoUsuario != null)
                    bdContext<ControleDeLoginEntities>.Instance.BD.DeleteObject(objGrupoUsuario);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool adicionarUsuarioAoGrupo(ref Grupos objGrupo, int userId, int idEstab)
        {
            try
            {                
                int idGrupo = objGrupo.Id;
                if (bdContext<ControleDeLoginEntities>.Instance.BD.GrupoUsuarios.Where(x => x.IdGrupo == idGrupo && 
                                                                                            x.IdUsuario == userId &&
                                                                                            x.IdEstabelecimento == idEstab).
                                                                                 Count() <= 0)
                {
                    GrupoUsuarios objGrupoUsuario = new GrupoUsuarios();
                    objGrupoUsuario.IdUsuario = userId;
                    objGrupoUsuario.IdEstabelecimento = idEstab;

                    objGrupo.GrupoUsuarios.Add(objGrupoUsuario);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool removerUsuarioDoGrupo(ref Grupos objGrupo, int userId, int idEstab)
        {
            try
            {
                int idGrupo = objGrupo.Id;
                GrupoUsuarios objGrupoUsuario = bdContext<ControleDeLoginEntities>.Instance.BD.GrupoUsuarios.Where(x => x.IdGrupo == idGrupo && 
                                                                                                                        x.IdUsuario == userId &&
                                                                                                                        x.IdEstabelecimento == idEstab).
                                                                                                             FirstOrDefault();

                if (objGrupoUsuario != null)
                    bdContext<ControleDeLoginEntities>.Instance.BD.DeleteObject(objGrupoUsuario);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool adicionarPermissaoAoGrupo(ref Grupos objGrupo, int visaoId, int tipoPerm)
        {
            try
            {
                int idGrupo = objGrupo.Id;

                List<GrupoVisoes> objGruposVisoes = bdContext<ControleDeLoginEntities>.Instance.BD.GrupoVisoes.Where(x => x.IdVisao == visaoId && x.IdGrupo == idGrupo).ToList();

                GrupoVisoes objGrupoVisao;
                if (objGruposVisoes.Count() > 0)
                    objGrupoVisao = objGruposVisoes.FirstOrDefault();
                else
                {
                    objGrupoVisao = new GrupoVisoes();
                    objGrupoVisao.IdVisao = visaoId;
                }

                if (objGrupoVisao.Permissao.Where(x => x.Tipo == tipoPerm).Count() <= 0)
                {
                    Permissao objPermissao = new Permissao();
                    objPermissao.Tipo = tipoPerm;
                    objGrupoVisao.Permissao.Add(objPermissao);

                    if (objGruposVisoes.Count() <= 0)
                        objGrupo.GrupoVisoes.Add(objGrupoVisao);

                    bdContext<ControleDeLoginEntities>.Instance.BD.SaveChanges();
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool removerPermissaoDoGrupo(ref Grupos objGrupo, int visaoId, int tipoPerm)
        {
            try
            {
                int idGrupo = objGrupo.Id;

                GrupoVisoes objGrupoVisoes = bdContext<ControleDeLoginEntities>.Instance.BD.GrupoVisoes.Where(x => x.IdGrupo == idGrupo && x.IdVisao == visaoId).FirstOrDefault();
                Permissao objPermissao = null;

                if (objGrupoVisoes != null)
                    objPermissao = objGrupoVisoes.Permissao.Where(x => x.Tipo == tipoPerm).FirstOrDefault();

                if (objPermissao != null)
                    bdContext<ControleDeLoginEntities>.Instance.BD.DeleteObject(objPermissao);

                if (objGrupoVisoes != null)
                {
                    if (objGrupoVisoes.Permissao.Count() <= 0)
                        bdContext<ControleDeLoginEntities>.Instance.BD.DeleteObject(objGrupoVisoes);
                }

                bdContext<ControleDeLoginEntities>.Instance.BD.SaveChanges();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsSenhaAntigaIgualBanco(ref Usuarios objUsuario, string senhaNaoCriptografada)
        {
            return objUsuario.Senha.Equals(getSenhaCriptografada(senhaNaoCriptografada));
        }

        public static bool IsSenhaNovaIgualSenhaRepetida(string senhaNova, string senhaNovaRepetida)
        {
            return senhaNova == senhaNovaRepetida;
        }

        public static bool removerTodosPrivilegiosGrupo(ref Grupos objGrupo)
        {
            try
            {
                List<GrupoVisoes> grpVisoes = objGrupo.GrupoVisoes.ToList();
                foreach (var grpVisao in grpVisoes)
                {
                    List<Permissao> permissoesVisao = grpVisao.Permissao.ToList();
                    foreach (var permissaoVisao in permissoesVisao)
                    {
                        bdContext<ControleDeLoginEntities>.Instance.BD.DeleteObject(permissaoVisao);
                    }
                    bdContext<ControleDeLoginEntities>.Instance.BD.DeleteObject(grpVisao);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool isAnyUsuarioVinculadoGrupo(ref Grupos objGrupo)
        {
            return objGrupo.GrupoUsuarios.Count() > 0;
        }

        public static bool isAnyGrupoVinculadoUsuario(ref Usuarios objUsuario)
        {
            return objUsuario.GrupoUsuarios.Count() > 0;
        }

        public static bool isOutroLoginMesmoNome(string newLogin, ref Usuarios objUsuario, int idSistema)
        {
            if (objUsuario != null)
            {
                if ((objUsuario.Login == newLogin) && (objUsuario.IdSistema == idSistema))
                    return false;
            }

            return bdContext<ControleDeLoginEntities>.
                   Instance.BD.Usuarios.
                   Where(x => x.Login == newLogin && 
                              x.IdSistema == idSistema).
                   Count() > 0;
        }

        public static Usuarios getUsuarioFromLoginSessions(string login, string senhaCriptografada, int idSistema)
        {
            return bdContext<ControleDeLoginEntities>.
                   Instance.BD.Usuarios.
                   Where(x => x.Login == login && 
                              x.Senha == senhaCriptografada && 
                              x.IdSistema == idSistema).
                   FirstOrDefault();
        }

        public static bool adicionarSistemaAoUsuario(ref Usuarios objUsuario, int idSistema)
        {
            try
            {
                objUsuario.IdSistema = idSistema;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool adicionarSistemaAoGrupo(ref Grupos objGrupo, int idSistema)
        {
            try
            {
                objGrupo.IdSistema = idSistema;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool haMaisDeUmEstabNosGruposUsuarios(ref Usuarios objUsuario)
        {
            List<ValDescr> tmpEstabs = null;

            if (!diferentesEstabsGruposUsuarios(ref objUsuario, out tmpEstabs))
                return false;

            return tmpEstabs.Count() > 1;
        }

        public static bool diferentesEstabsGruposUsuarios(ref Usuarios objUsuario, out List<ValDescr> estabs)
        {
            estabs = null;
            try
            {
                estabs = objUsuario.GrupoUsuarios.Select(x => new ValDescr
                                                                {
                                                                    Id = (x.Estabelecimentos == null) ? -1 : x.IdEstabelecimento.Value,
                                                                    Descricao = (x.Estabelecimentos == null) ? "" : x.Estabelecimentos.NomeFantasia
                                                                }
                                                         ).Distinct().
                                                           ToList();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool getConnectionAliasByIdEstab(int idEstab, out string ConnectionAlias)
        {
            ConnectionAlias = "";
            try
            {
                ConnectionAlias = bdContext<ControleDeLoginEntities>.Instance.BD.
                                            Estabelecimentos.Where(x => x.Id.Equals(idEstab)).
                                            FirstOrDefault().AliasConnection;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool adicionarSistemaAoEstab(ref Estabelecimentos objEstab, int idSistema)
        {
            try
            {
                objEstab.IdSistema = idSistema;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool criptografarAES(string textoParaEncriptar, out string textoEncriptado)
        {
            try
            {
                AESEncriptor aesEncriptor = new AESEncriptor();

                aesEncriptor.Key = getAesKey();
                aesEncriptor.InitializatorVector = getAesIV();
                aesEncriptor.PlainText = textoParaEncriptar;

                byte[] encriptedData = null;
                if (!aesEncriptor.encriptarAES(out encriptedData))
                {
                    textoEncriptado = null;
                    return false;
                }

                textoEncriptado = Convert.ToBase64String(encriptedData);

                return true;
            }
            catch
            {
                textoEncriptado = null;
                return false;
            }
        }

        public static bool descriptografarAES(string textoParaDescriptografar, out string textoDescriptografado)
        {
            try
            {
                AESEncriptor aesEncriptor = new AESEncriptor();                

                aesEncriptor.Key = getAesKey();
                aesEncriptor.InitializatorVector = getAesIV();                
                aesEncriptor.CipherText = Convert.FromBase64String(textoParaDescriptografar);

                string decriptedData = null;
                if (!aesEncriptor.decriptarAES(out decriptedData))
                {
                    textoDescriptografado = null;
                    return false;
                }

                textoDescriptografado = decriptedData;

                return true;
            }
            catch
            {
                textoDescriptografado = null;
                return false;
            }
        }

        private static int AdicionarTentativa(ref int tentativas)
        {
            return tentativas++;
        }

        private static void BloqueiaUsuario(string login, int idSistema)
        {
            bdContext<ControleDeLoginEntities>.
                Instance.BD.Usuarios.
                Where(x => x.Login == login && x.IdSistema == idSistema).
                FirstOrDefault().Bloqueado = true;

            bdContext<ControleDeLoginEntities>.Instance.BD.SaveChanges();
        }

        public static int getIdSistema(string sistema)
        {
            return bdContext<ControleDeLoginEntities>.
                             Instance.BD.Sistemas.Where(x => x.Descricao == sistema).
                             FirstOrDefault().Id;
        }

        public static void VerificarSeBloqueiaUsuario(ref int tentativas, string login,
                                                      string sistema)
        {
            if (AdicionarTentativa(ref tentativas) >= 5)
                BloqueiaUsuario(login, getIdSistema(sistema));
        }

        public static Usuarios getUsuarioById(int idUsuario)
        {
            return bdContext<ControleDeLoginEntities>.Instance.BD.Usuarios.Where(x => x.Id.Equals(idUsuario)).FirstOrDefault();
        }
    }
}