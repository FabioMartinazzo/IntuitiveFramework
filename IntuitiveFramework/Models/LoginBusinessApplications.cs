﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Security.Cryptography;

namespace IntuitiveFramework.Models
{
    public static class LoginBusinessApplications
    {
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
    }
}