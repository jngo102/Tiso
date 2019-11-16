using System;
using System.Collections;
using System.Collections.Generic;
using ModCommon;
using UnityEngine;
using Random = System.Random;

namespace Tiso
{
    public class TisoAttacks : MonoBehaviour
    {
        private const int CollisionMask = 1 << 8;
        private const float DashVelocity = 30.0f;
        private const float EvadeVelocity = 40.0f;
        private const float Extension = 1.0f;
        private const float Gravity = 90f;
        private const float GroundY = 5.0f;
        private const float JumpVelocity = 40f;
        private const float LeftX = 52.9f;
        private const float RightX = 70.9f;
        
        private static List<Action> _moves = new List<Action>();
        
        private static TisoAnimator _anim;
        private static TisoAudio _audio;
        private static TisoAttacks _instance;
        private static PhaseControl _phaseCtrl;
        private static Random _rand;
        private static Rigidbody2D _rb;
        private static Recoil _recoil;
        private static SpriteRenderer _sr;

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
        }

        private void OnTriggeredPhase2()
        {
            Log("Handle Phase 2");
            _moves.Add(TisoThrow);
        }
        
        private void OnTriggeredPhase3()
        {
            Log("Handle Phase 3");
            _moves.Add(TisoLaser);
        }
        
        private IEnumerator Start()
        {
            yield return null;

            while (HeroController.instance == null) yield return null;

            Log("Tiso Start");

            _moves.Clear();
            _moves.Add(TisoDash);
            _moves.Add(TisoJump);
            _moves.Add(TisoSlash);
            
            yield return new WaitForSeconds(1.0f);

            StartCoroutine(IdleAndChooseNextAttack());
        }
        
        private void Update()
        {
            /*foreach (var move in _moves)
            {
                Log("Move: " + move.Method.Name);   
            }
            Log("");*/
        }

        private void FixedUpdate()
        {
            if (!IsGrounded())
            {
                _rb.velocity += Vector2.down * Gravity * Time.deltaTime;
            }
            else
            {
                FaceKnight();
            }

            CheckWallCollisions();
        }

        private void LateUpdate()
        {
        }

        private IEnumerator IdleAndChooseNextAttack()
        {
            _anim.PlayAnimation("Idle", true);
            _recoil.SetRecoilSpeed(15f);
            
            float minWait = 0.5f;
            float maxWait = 1.0f;
            float waitTime = (float) (_rand.NextDouble() * maxWait) + minWait;

            yield return new WaitForSeconds(waitTime);

            int index = _rand.Next(_moves.Count);
            Action nextMove = _moves[index];

            if (Math.Abs(transform.position.x - HeroController.instance.transform.position.x) < 2.0f)
            {
                nextMove = TisoEvade;
            }
            
            Log("Next Move: " + nextMove.Method.Name);
            try
            {
                nextMove.Invoke();
            }
            catch (Exception e)
            {
                Log(e);
                throw;
            }
        }
        
        public void TisoDash()
        {
            IEnumerator DashAntic()
            {
                _anim.PlayAnimation("Dash Antic");
                
                yield return new WaitForSeconds(_anim.GetAnimDuration("Dash Antic"));

                StartCoroutine(Dashing(0.25f));
            }

            IEnumerator Dashing(float dashTime)
            {
                _anim.PlayAnimation("Dashing", true);
                _audio.PlayAudioClip("Dash");
                _rb.velocity = new Vector2(DashVelocity * direction, 0);
                _recoil.freezeInPlace = true;
                
                yield return new WaitForSeconds(dashTime);

                StartCoroutine(DashRecover());
            }

            IEnumerator DashRecover()
            {
                _anim.PlayAnimation("Dash Recover");
                if (_rb.velocity.x > 0)
                {
                    do
                    {
                        _rb.velocity += Vector2.left * 0.1f * DashVelocity * (1.0f / _anim.GetAnimDuration("Dash Recover"));
                        yield return new WaitForSeconds(0.1f);
                    } 
                    while (_rb.velocity.x < 0);
                }
                else if (_rb.velocity.x < 0)
                {
                    do
                    {
                        _rb.velocity += Vector2.right * 0.1f * DashVelocity * (1.0f / _anim.GetAnimDuration("Dash Recover"));
                        yield return new WaitForSeconds(0.1f);
                    } 
                    while (_rb.velocity.x < 0);
                }

                _rb.velocity = Vector2.zero;

                StartCoroutine(IdleAndChooseNextAttack());
            }

            StartCoroutine(DashAntic());
        }

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
                _rb.velocity = new Vector2(_rand.Next(10, 20) * direction, JumpVelocity);
                _recoil.SetRecoilSpeed(0.0f);
                
                yield return null;
                
                StartCoroutine(Jumping());
            }

            IEnumerator Jumping()
            {
                Log("Jumping");
                while (transform.position.y > GroundY && !IsGrounded() || _rb.velocity.y >= 0)
                {
                    yield return null;
                }
                StartCoroutine(Land());
            }

            IEnumerator Land()
            {
                Log("Land");
                _rb.velocity = Vector2.zero;
                transform.SetPosition2D(transform.position.x, GroundY);
                _audio.PlayAudioClip("Land");
                _anim.PlayAnimation("Land");
                
                yield return new WaitForSeconds(_anim.GetAnimDuration("Land"));
                
                StartCoroutine(JumpRecover());
            }

            IEnumerator JumpRecover()
            {
                Log("Jump Recover");
                yield return null;

                StartCoroutine(IdleAndChooseNextAttack());
            }
            
            StartCoroutine(JumpAntic());
        }

        private void TisoSlash()
        {
            StartCoroutine(IdleAndChooseNextAttack());
        }

        private GameObject _shieldGO;
        private Shield _shieldComponent;
        private Coroutine _throwWaitRoutine;
        private void TisoThrow()
        {
            IEnumerator ThrowAntic()
            {
                _anim.PlayAnimation("Throw Antic", true);
                yield return new WaitForSeconds(0.5f);

                StartCoroutine(Throw());
            }

            IEnumerator Throw()
            {
                _anim.PlayAnimation("Throw");
                _shieldGO = Instantiate(new GameObject("Shield"), transform.position, Quaternion.identity);
                _shieldComponent = _shieldGO.AddComponent<Shield>();
                _shieldComponent.direction = direction;
                yield return new WaitForSeconds(_anim.GetAnimDuration("Throw"));
                
                _throwWaitRoutine = StartCoroutine(ThrowWait());
            }

            IEnumerator ThrowWait()
            {
                _anim.PlayAnimation("Throw Wait", true);
                _recoil.SetRecoilSpeed(0.0f);

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
                _anim.PlayAnimation("Throw Recover");
               yield return new WaitForSeconds(_anim.GetAnimDuration("Throw Recover"));
               
               _shieldComponent.ReturnedToTiso -= OnReturnedToTiso;

               StartCoroutine(IdleAndChooseNextAttack());
            }

            StartCoroutine(ThrowAntic());
        }

        private void TisoLaser()
        {
            StartCoroutine(IdleAndChooseNextAttack());
        }

        private void TisoEvade()
        {
            IEnumerator EvadeAntic()
            {
                _anim.PlayAnimation("Evade Antic");
                
                yield return new WaitForSeconds(_anim.GetAnimDuration("Evade Antic"));

                StartCoroutine(Evade(0.15f));
            }

            IEnumerator Evade(float evadeTime)
            {
                _anim.PlayAnimation("Evading");
                _rb.velocity = Vector2.left * EvadeVelocity * direction;
             
                yield return new WaitForSeconds(evadeTime);

                StartCoroutine(EvadeLand());
            }

            IEnumerator EvadeLand()
            {
                _anim.PlayAnimation("Evade Land");
                _audio.PlayAudioClip("Evade Land");
                _rb.velocity = Vector2.zero;
                yield return new WaitForSeconds(_anim.GetAnimDuration("Evade Land"));

                StartCoroutine(IdleAndChooseNextAttack());
            }

            StartCoroutine(EvadeAntic());
        }

        private void TisoCounter()
        {
            StartCoroutine(IdleAndChooseNextAttack());
        }
        
       private bool IsGrounded()
        {
            float rayLength = _sr.bounds.extents.y + Extension;
            /*GameObject line = new GameObject();
            LineRenderer lineRenderer = line.AddComponent<LineRenderer>();
            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.1f;
            lineRenderer.startColor = Color.red;
            lineRenderer.endColor = Color.red;
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, transform.position + Vector3.down * rayLength);
            Destroy(line, 0.01f);*/
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

       private int direction = 1;
       private void FaceKnight()
       {
           float heroX = HeroController.instance.transform.GetPositionX();
           Transform trans = transform;
           float tisoX = trans.position.x;
           if (heroX - tisoX > 0)
           {
               _sr.flipX = true;
               direction = 1;
           }
           else
           {
               _sr.flipX = false;
               direction = -1;
           }
       }

        private static void Log(object message) => Modding.Logger.Log("[Tiso Attacks]: " + message);
    }
}