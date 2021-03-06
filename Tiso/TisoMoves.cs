﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using HutongGames.PlayMaker.Actions;
using ModCommon;
using ModCommon.Util;
using UnityEngine;
using Random = System.Random;

namespace Tiso
{
    public class TisoMoves : MonoBehaviour
    {
        private const int CollisionMask = 1 << 8;
        private const float DashVelocity = 30.0f;
        private const float DiveVelocity = 50.0f;
        private const float EvadeVelocity = 40.0f;
        private const float Extension = 0.1f;
        private const float Gravity = 90f;
        private const float GroundY = 5.0f;
        private const float JumpVelocity = 40f;
        private const float LeftX = 52.2f;
        private const float RightX = 71.6f;
        private const float SlashVelocity = 45.0f;
        
        private List<Action> _moves;
        private Dictionary<Action, int> _repeats;
        
        private TisoAnimator _anim;
        private TisoAudio _audio;
        private TisoMoves _instance;
        private PhaseControl _phaseCtrl;
        private Random _rand;
        private Rigidbody2D _rb;
        private Recoil _recoil;
        private SpriteRenderer _sr;

        private GameObject _bee;
        private GameObject _hornet;
        private GameObject _kin;
        private PlayMakerFSM _mageLord;

        private void Awake()
        {
            _instance = this;
            
            _anim = GetComponent<TisoAnimator>();
            _audio = GetComponent<TisoAudio>();
            _rand = new Random();
            _rb = GetComponent<Rigidbody2D>();
            _recoil = GetComponent<Recoil>();
            _sr = GetComponent<SpriteRenderer>();
            _phaseCtrl = GetComponent<PhaseControl>();

            _phaseCtrl.TriggeredPhase2 += OnTriggeredPhase2;
            _phaseCtrl.TriggeredPhase3 += OnTriggeredPhase3;

            HeroController.instance.OnDeath += OnHeroDeath;

            _bee = TisoSpencer.PreloadedGameObjects["Bee"];
            _hornet = TisoSpencer.PreloadedGameObjects["Hornet"];
            _kin = TisoSpencer.PreloadedGameObjects["Kin"];
            _mageLord = TisoSpencer.PreloadedGameObjects["Mage"].LocateMyFSM("Mage Lord");
        }

        private void OnTriggeredPhase2()
        {
            Log("Handle Phase 2");
            _moves.Add(TisoShieldThrow);
            _repeats[TisoShieldThrow] = 0;

            _moves.Add(TisoBombThrow);
            _repeats[TisoBombThrow] = 0;
        }
        
        private void OnTriggeredPhase3()
        {
            Log("Handle Phase 3");
            _moves.Add(TisoLaser);
            _repeats[TisoLaser] = 0;
        }

        private bool _knightDead;
        private void OnHeroDeath()
        {
            _knightDead = true;
        }
        
        private IEnumerator Start()
        {
            yield return null;

            while (HeroController.instance == null) yield return null;

            Log("Tiso Start");

            _moves = new List<Action>
            {
                TisoDash,
                TisoJump,
                TisoSlash,
            };

            _repeats = new Dictionary<Action, int>
            {
                [TisoDash] = 0,
                [TisoJump] = 0,
                [TisoSlash] = 0,
            };
            
            _anim.PlayAnimation("Raise to Idle");
            yield return new WaitForSeconds(_anim.GetAnimDuration("Raise to Idle"));

            StartIdle();
        }

        private void FixedUpdate()
        {
            if (!IsGrounded() && !_isDiving)
            {
                _rb.velocity += Vector2.down * Gravity * Time.deltaTime;
            }

            CheckWallCollisions();
        }

        private Coroutine _idling;
        private IEnumerator IdleAndChooseNextAttack()
        {
            Log("Idle");
            _anim.PlayAnimation("Idle", true);
            _recoil.SetRecoilSpeed(15f);
            
            StartCoroutine(FaceKnight());
            
            float minWait = 0.25f;
            float maxWait = 0.5f;
            float waitTime = (float) (_rand.NextDouble() * maxWait) + minWait;
            
            yield return new WaitForSeconds(waitTime);
            
            int index = _rand.Next(_moves.Count);
            Action nextMove = _moves[index];
            
            // Make sure moves don't occur more than twice in a row
            while (_repeats[nextMove] >= 2)
            {
                index = _rand.Next(_moves.Count);
                Log("Index: " + index);
                nextMove = _moves[index];
            }

            foreach (Action move in _moves)
            {
                if (move == nextMove)
                {
                    _repeats[move]++;
                }
                else
                {
                    _repeats[move] = 0;
                }
            }
            
            Vector2 pos = transform.position;
            Vector2 heroPos = HeroController.instance.transform.position;
            if (Mathf.Sqrt(Mathf.Pow(pos.x - heroPos.x, 2) + Mathf.Pow(pos.y - heroPos.y, 2)) < 4.0f)
            {
                int randNum = _rand.Next(100);
                int threshold = 70;
                if (randNum < threshold)
                {
                    if (_direction == 1 && pos.x - LeftX > 3.0f || (_direction == -1 && RightX - pos.x > 3.0f))
                    {
                        nextMove = TisoEvade;
                    }
                }
            }
            else if (Mathf.Abs(pos.x - heroPos.x) <= 2.0f && heroPos.y - pos.y > 2.0f)
            {
                int randNum = _rand.Next(100);
                int threshold = 50;
                if (randNum < threshold)
                {
                    nextMove = TisoUpslash;   
                }
            }

            if (_knightDead)
            {
                nextMove = TisoDab;
            }

            Log("Next Move: " + nextMove.Method.Name);
            nextMove.Invoke();
        }
        
        public void TisoDash()
        {
            IEnumerator DashAntic()
            {
                Log("Dash Antic");
                _anim.PlayAnimation("Dash Antic");

                yield return new WaitForSeconds(_anim.GetAnimDuration("Dash Antic"));

                StartCoroutine(Dashing(0.25f));
            }

            IEnumerator Dashing(float dashTime)
            {
                Log("Dashing");
                _anim.PlayAnimation("Dashing", true);
                _audio.PlayAudioClip("Dash");
                _rb.velocity = new Vector2(DashVelocity * _direction, 0);
                _recoil.SetRecoilSpeed(0.0f);

                GameObject gDashEFfect = Instantiate(TisoSpencer.PreloadedGameObjects["G Dash"], transform.position + Vector3.right * -4.0f * _direction, Quaternion.identity);
                gDashEFfect.SetActive(true);

                gDashEFfect.transform.rotation = Quaternion.Euler(0, Mathf.Clamp(180 * _direction, 0, 180), 0);
                
                PlayMakerFSM fsm = gDashEFfect.LocateMyFSM("FSM");
                Destroy(fsm);
                
                tk2dSpriteAnimator anim = gDashEFfect.GetComponent<tk2dSpriteAnimator>();
                anim.PlayFromFrame(0);
                Destroy(gDashEFfect, 0.25F);

                GameCameras.instance.cameraShakeFSM.SendEvent("EnemyKillShake");
                
                yield return new WaitForSeconds(dashTime);

                StartCoroutine(DashRecover());
            }

            IEnumerator DashRecover()
            {
                Log("Dash Recover");
                _anim.PlayAnimation("Dash Recover");
                while (Mathf.Abs(_rb.velocity.x) >= 2.0f)
                {
                    Log("Velocity X: " + _rb.velocity.x);
                    _rb.velocity += Vector2.right * -_direction * 0.1f * (DashVelocity / _anim.GetAnimDuration("Dash Recover"));
                    yield return new WaitForSeconds(0.1f);
                }

                StartCoroutine(DashEnd());
            }

            IEnumerator DashEnd()
            {
                Log("Dash End");
                _rb.velocity = Vector2.zero;

                yield return null;

                StartIdle();
            }

            StartCoroutine(DashAntic());
        }

        private bool _isDiving;
        public void TisoJump()
        {
            IEnumerator JumpAntic()
            {
                Log("Jump Antic");
                _rb.velocity = Vector2.zero;
                _anim.PlayAnimation("Jump Antic");
                
                yield return new WaitForSeconds(_anim.GetAnimDuration("Jump Antic"));
                
                StartCoroutine(Jump());
            }

            IEnumerator Jump()
            {
                Log("Jump");
                _audio.PlayAudioClip("Jump");
                _anim.PlayAnimation("Spinning", true);
                _rb.velocity = new Vector2(_rand.Next(10, 20) * _direction, JumpVelocity);
                _recoil.SetRecoilSpeed(0.0f);
                
                GameObject ptJump = Instantiate(TisoSpencer.PreloadedGameObjects["Pt Jump"], transform.position += Vector3.down * (_sr.bounds.extents.y / 2), Quaternion.identity);
                ptJump.SetActive(true);
                ParticleSystem ps = ptJump.GetComponent<ParticleSystem>();
                ps.Play();


                yield return null;
                
                
                StartCoroutine(Jumping());
            }
            
            IEnumerator Jumping()
            {
                Log("Jumping");
                bool willDive = false;
                _isDiving = false;
                float diveWeight = _rand.Next(0, 100);
                if (diveWeight <= 50)
                {
                    willDive = true;
                }
                while (transform.position.y > GroundY && !IsGrounded() || _rb.velocity.y >= 0)
                {
                    float heroX = HeroController.instance.transform.position.x;
                    float tisoX = transform.position.x;
                    float distX = heroX - tisoX;
                    if (Mathf.Abs(distX) < 0.5f)
                    {
                        if (willDive)
                        {
                            _isDiving = true;
                            TisoDive();
                            break;
                        }
                    }
                    
                    yield return null;
                }
                
                if (!_isDiving) StartCoroutine(Land());
            }
            IEnumerator Land()
            {
                Log("Land");
                _rb.velocity = Vector2.zero;
                transform.SetPosition2D(transform.position.x, GroundY);
                _audio.PlayAudioClip("Land");
                _anim.PlayAnimation("Land");
                
                GameObject ptLand = gameObject.FindGameObjectInChildren("Pt Land");
                GameObject slamEffect = gameObject.FindGameObjectInChildren("Slam Effect");
                
                Instantiate(ptLand, transform.position + Vector3.down * (_sr.bounds.extents.y / 2), Quaternion.identity).SetActive(true);
                Instantiate(slamEffect, transform.position + Vector3.down * (_sr.bounds.extents.y / 2), Quaternion.identity).SetActive(true);
                
                yield return new WaitForSeconds(_anim.GetAnimDuration("Land"));
                
                StartCoroutine(JumpRecover());
            }

            IEnumerator JumpRecover()
            {
                Log("Jump Recover");
                yield return null;

                StartIdle();
            }
            
            StartCoroutine(JumpAntic());
        }

        private void TisoSlash()
        {
            IEnumerator SlashAntic()
            {
                Log("Slash Antic");
                _anim.PlayAnimation("Slash Antic");

                yield return new WaitForSeconds(_anim.GetAnimDuration("Slash Antic"));

                StartCoroutine(Slash1());
            }

            GameObject slash1;
            IEnumerator Slash1()
            {
                Log("Slash 1");
                _anim.PlayAnimation("Slash 1");
                _audio.PlayAudioClip("Slash");
                _rb.velocity = Vector2.right * _direction * SlashVelocity;
                _recoil.SetRecoilSpeed(0.0f);
                slash1 = Instantiate(TisoSpencer.PreloadedGameObjects["Slash"], transform.position + Vector3.right * _direction * 1.0f, Quaternion.identity);
                slash1.SetActive(true);
                slash1.layer = 22;
                slash1.AddComponent<DamageHero>();
                slash1.AddComponent<DebugColliders>();
                Rigidbody2D rb = slash1.AddComponent<Rigidbody2D>();
                rb.velocity = _rb.velocity;
                PolygonCollider2D slashCollider = slash1.GetComponent<PolygonCollider2D>();
                Vector2[] points =
                {
                    new Vector2(2.5f * _direction, -1.0f),
                    new Vector2(2.5f * _direction, 1.0f),
                    new Vector2(-2.5f * _direction, 1.0f),
                    new Vector2(-2.5f * _direction, -1.0f),
                };
                slashCollider.points = points;
                slashCollider.SetPath(0, points);

                yield return new WaitForSeconds(_anim.GetAnimDuration("Slash 1"));

                StartCoroutine(Slash1Pause());
            }

            IEnumerator Slash1Pause()
            {
                Log("Slash 1 Pause");
                Destroy(slash1);
                _rb.velocity = Vector2.zero;
                
                yield return new WaitForSeconds(0.1f);

                StartCoroutine(Slash2Antic());
            }

            GameObject slash2;
            IEnumerator Slash2Antic()
            {
                Log("Slash 2 Antic");
                _anim.PlayAnimation("Slash 2 Antic");

                yield return new WaitForSeconds(_anim.GetAnimDuration("Slash 2 Antic"));

                StartCoroutine(Slash2());
            }
            
            IEnumerator Slash2()
            {
                Log("Slash 2");
                _anim.PlayAnimation("Slash 2");
                _audio.PlayAudioClip("Slash");
                _rb.velocity = Vector2.right * _direction * SlashVelocity;
                slash2 = Instantiate(TisoSpencer.PreloadedGameObjects["Slash"], transform.position + Vector3.right * _direction * 1.0f, Quaternion.identity);
                slash2.SetActive(true);
                slash2.layer = 22;
                slash2.AddComponent<DamageHero>();
                slash2.AddComponent<DebugColliders>();
                Rigidbody2D rb = slash2.AddComponent<Rigidbody2D>();
                rb.velocity = _rb.velocity; 
                PolygonCollider2D slashCollider = slash2.GetComponent<PolygonCollider2D>();
                Vector2[] points =
                {
                    new Vector2(2.8f * _direction, -1.0f),
                    new Vector2(2.8f * _direction, 1.0f),
                    new Vector2(-2.5f * _direction, 1.0f),
                    new Vector2(-2.5f * _direction, -1.0f),
                };
                slashCollider.points = points;
                slashCollider.SetPath(0, points);

                yield return new WaitForSeconds(_anim.GetAnimDuration("Slash 2"));

                StartCoroutine(Slash2Pause());
            }

            IEnumerator Slash2Pause()
            {
                Log("Slash 2 Pause");
                Destroy(slash2);
                _rb.velocity = Vector2.zero;
                
                yield return new WaitForSeconds(0.1f);

                StartCoroutine(SlashRecover());
            }

            IEnumerator SlashRecover()
            {
                Log("Slash Recover");
                _anim.PlayAnimation("Slash Recover");

                yield return new WaitForSeconds(_anim.GetAnimDuration("Slash Recover"));

                StartIdle();
            }
            
            StartCoroutine(SlashAntic());
        }

        private GameObject _shieldGO;
        private Shield _shieldComponent;
        private void TisoShieldThrow()
        {
            IEnumerator ThrowAntic()
            {
                _anim.PlayAnimation("Shield Throw Antic", true);
                _recoil.SetRecoilSpeed(0.0f);
                yield return new WaitForSeconds(_anim.GetAnimDuration("Shield Throw Antic"));

                StartCoroutine(Throw());
            }

            IEnumerator Throw()
            {
                _anim.PlayAnimation("Shield Throw");
                Vector3 shieldPos = transform.position + Vector3.down;
                _shieldGO = Instantiate(new GameObject("Shield"), shieldPos, Quaternion.identity);
                _shieldComponent = _shieldGO.AddComponent<Shield>();
                _shieldComponent.direction = _direction;
                yield return new WaitForSeconds(_anim.GetAnimDuration("Shield Throw"));
                
                StartCoroutine(ThrowWait());
            }

            IEnumerator ThrowWait()
            {
                _anim.PlayAnimation("Shield Throw Wait", true);

                yield return null;
                
                _shieldComponent.ReturnedToTiso += OnReturnedToTiso;
            }

            void OnReturnedToTiso()
            {
                Log("Returned to Tiso");
                StartCoroutine(ThrowRecover());
            }
            
            IEnumerator ThrowRecover()
            {
                Destroy(_shieldGO);
                _anim.PlayAnimation("Shield Throw Recover");
               yield return new WaitForSeconds(_anim.GetAnimDuration("Shield Throw Recover"));

               _shieldComponent.ReturnedToTiso -= OnReturnedToTiso;

               StartIdle();
            }

            StartCoroutine(ThrowAntic());
        }

        private void TisoLaser()
        {
            IEnumerator LaserAntic()
            {
                _anim.PlayAnimation("Dash Antic");
                
                yield return null;

                StartCoroutine(Laser());
            }

            IEnumerator Laser()
            {
                try
                {
                    GameObject laser = Instantiate(TisoSpencer.PreloadedGameObjects["Laser"], transform.position, Quaternion.identity);
                    laser.SetActive(true);
                    Destroy(laser.LocateMyFSM("Laser Bug Mega"));
                    laser.PrintSceneHierarchyTree();
                }
                catch (Exception e)
                {
                    Log(e);
                }

                yield return new WaitForSeconds(2.0f);

                StartIdle();
            }
            
            StartCoroutine(LaserAntic());
        }

        private void TisoEvade()
        {
            IEnumerator EvadeAntic()
            {
                _anim.PlayAnimation("Evade Antic");
                
                yield return new WaitForSeconds(_anim.GetAnimDuration("Evade Antic"));

                StartCoroutine(Evade(0.25f));
            }

            IEnumerator Evade(float evadeTime)
            {
                _anim.PlayAnimation("Evading");
                _rb.velocity = Vector2.left * EvadeVelocity * _direction;
             
                yield return new WaitForSeconds(evadeTime);

                StartCoroutine(EvadeLand());
            }

            IEnumerator EvadeLand()
            {
                _anim.PlayAnimation("Evade Land");
                _audio.PlayAudioClip("Evade Land");
                _rb.velocity = Vector2.zero;
                yield return new WaitForSeconds(_anim.GetAnimDuration("Evade Land"));

                StartIdle();
            }

            StartCoroutine(EvadeAntic());
        }

        private void TisoCounter()
        {
            StartIdle();
        }

        private void TisoUpslash()
        {
            IEnumerator UpslashAntic()
            {
                Log("Upslash Antic");
                _anim.PlayAnimation("Upslash Antic");
                _recoil.SetRecoilSpeed(0.0f);
                
                yield return new WaitForSeconds(_anim.GetAnimDuration("Upslash Antic"));

                StartCoroutine(Upslash());
            }

            GameObject upslash;
            IEnumerator Upslash()
            {
                Log("Upslash");
                transform.position += Vector3.up * 2.5f;
                _anim.PlayAnimation("Upslash");
                _audio.PlayAudioClip("Slash");

                upslash = Instantiate(TisoSpencer.PreloadedGameObjects["Slash"], transform.position, Quaternion.identity);
                upslash.SetActive(true);
                upslash.layer = 22;
                upslash.AddComponent<DamageHero>();
                Vector2[] points =
                {
                    new Vector2(-1.5f, -2.0f),
                    new Vector2(-1.5f, 4.0f),
                    new Vector2(1.5f, 4.0f),
                    new Vector2(1.5f, -2.0f),
                };
                PolygonCollider2D upslashCollider = upslash.GetComponent<PolygonCollider2D>(); 
                upslashCollider.points = points;
                upslash.AddComponent<DebugColliders>();

                yield return new WaitForSeconds(_anim.GetAnimDuration("Upslash"));

                StartCoroutine(UpslashPause());
            }

            IEnumerator UpslashPause()
            {
                Destroy(upslash);

                yield return new WaitForSeconds(0.1f);

                StartCoroutine(UpslashRecover());
            }
            
            IEnumerator UpslashRecover()
            {
                transform.position += Vector3.down * 2.5f;
                
                yield return null;

                StartIdle();
            }

            StartCoroutine(UpslashAntic());
        }

        private void TisoBombThrow()
        {
            IEnumerator BombThrowAntic()
            {
                _anim.PlayAnimation("Bomb Throw Antic");
                
                yield return new WaitForSeconds(_anim.GetAnimDuration("Bomb Throw Antic"));

                StartCoroutine(BombThrow());
            }

            IEnumerator BombThrow()
            {
                _anim.PlayAnimation("Bomb Throw");

                GameObject bomb = Instantiate(new GameObject("Bomb"), transform.position, Quaternion.identity);
                bomb.AddComponent<Bomb>();
                
                yield return new WaitForSeconds(_anim.GetAnimDuration("Bomb Throw"));
                
                StartIdle();
            }
            
            StartCoroutine(BombThrowAntic());
        }

        private void TisoDive()
        {
            IEnumerator DiveAntic()
            {
                _anim.PlayAnimation("Dive Antic");
                _rb.velocity = Vector2.zero;
                
                yield return new WaitForSeconds(_anim.GetAnimDuration("Dive Antic"));

                StartCoroutine(Diving());
            }

            IEnumerator Diving()
            {
                _anim.PlayAnimation("Diving", true);
                _audio.PlayAudioClip("Diving", 0.25f);
                
                _rb.velocity = Vector2.down * DiveVelocity;
                
                while (!IsGrounded())
                {
                    yield return null;
                }

                StartCoroutine(DiveLand());
            }

            IEnumerator DiveLand()
            {
                _anim.PlayAnimation("Dive Land");
                _audio.PlayAudioClip("Dive Land", 0.25f);
                _rb.velocity = Vector2.zero;
                Log("Calling SpawnShockwaves");
                SpawnShockwaves(1.0f, 50f, 1);
                GameCameras.instance.cameraShakeFSM.SendEvent("SmallShake");
            
                yield return new WaitForSeconds(_anim.GetAnimDuration("Dive Land"));

                StartIdle();
            }

            StartCoroutine(DiveAntic());
        }
        
        private void TisoDab()
        {
            IEnumerator DabAntic()
            {
                _anim.PlayAnimation("Dab Antic");

                yield return new WaitForSeconds(_anim.GetAnimDuration("Dab Antic"));

                StartCoroutine(Dab());
            }

            IEnumerator Dab()
            {
                _anim.PlayAnimation("Dab");
                transform.position += Vector3.down;
                
                yield return new WaitForSeconds(_anim.GetAnimDuration("Dab"));
            }

            StartCoroutine(DabAntic());
        }

        private void SpawnShockwaves(float vertScale, float speed, int damage)
        {
            bool[] facingRightBools = {false, true};
            Vector2 pos = transform.position;
            foreach (bool facingRight in facingRightBools)
            {
                Log("Instantiating Shockwave");
                GameObject shockwave =
                    Instantiate(_mageLord.GetAction<SpawnObjectFromGlobalPool>("Quake Waves").gameObject.Value);
                Log("Getting shockwave FSM");
                PlayMakerFSM shockFSM = shockwave.LocateMyFSM("shockwave");
                shockFSM.FsmVariables.FindFsmBool("Facing Right").Value = facingRight;
                shockFSM.FsmVariables.FindFsmFloat("Speed").Value = speed;
                shockwave.AddComponent<DamageHero>().damageDealt = damage;
                Log("Setting shockwave active");
                shockwave.SetActive(true);
                shockwave.transform.SetPosition2D(new Vector2(pos.x + (facingRight ? 1.5f : -1.5f), 3.0f));
                shockwave.transform.SetScaleY(vertScale);
            }

        }
        
       private bool IsGrounded()
        {
            float rayLength = _sr.bounds.extents.y + Extension;
            return Physics2D.Raycast(transform.position, Vector2.down, rayLength, CollisionMask);
        }

       private void CheckWallCollisions()
       {
           if (transform.position.x <= LeftX)
           {
               transform.SetPositionX(LeftX);
           }
           else if (transform.position.x >= RightX)
           {
               transform.SetPositionX(RightX);
           }

           if (transform.position.y <= GroundY)
           {
               transform.SetPositionY(GroundY);
           }
       }

       private int _direction = -1;
       private IEnumerator FaceKnight()
       {
           float heroX = HeroController.instance.transform.GetPositionX();
           Transform trans = transform;
           float tisoX = trans.position.x;
           if (heroX - tisoX > 0 && _direction == -1)
           {
               _anim.PlayAnimation("Turn");
               yield return new WaitForSeconds(_anim.GetAnimDuration("Turn"));
               _sr.flipX = true;
               _direction = 1;
               StartIdle();
           }
           else if (heroX - tisoX < 0 && _direction == 1)
           {
               _anim.PlayAnimation("Turn");
               yield return new WaitForSeconds(_anim.GetAnimDuration("Turn"));
               _sr.flipX = false;
               _direction = -1;
               _anim.PlayAnimation("Idle", true);
               if (_idling != null) StopCoroutine(_idling);
               StartIdle();
           }
       }

       private void StartIdle()
       {
           if (_idling != null) StopCoroutine(_idling);
           _idling = StartCoroutine(IdleAndChooseNextAttack());
       }
       
        private static void Log(object message) => Modding.Logger.Log("[Tiso Attacks]: " + message);
    }
}