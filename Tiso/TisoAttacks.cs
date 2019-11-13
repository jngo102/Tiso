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
        private const float AnimFPS = 1.0f / 12;
        private const int CollisionMask = 1 << 8;
        private const float Extension = 1.0f;
        private const float Gravity = 60f;
        private const float GroundY = 5.0f;
        private const float JumpVelocity = 30f;
        private const float LeftX = 52.9f;
        private const float RightX = 70.9f;
     
        private static Dictionary<string, Action> _moves = new Dictionary<string, Action>();
        
        private static TisoAnimator _anim;
        private static TisoAudio _audio;
        private static TisoAttacks _instance;
        private static PhaseControl _phaseCtrl;
        private static Random _rand;
        private static Rigidbody2D _rb;
        private static SpriteRenderer _sr;

        private void Awake()
        {
            _instance = this;
            
            _anim = GetComponent<TisoAnimator>();
            _audio = GetComponent<TisoAudio>();
            _rand = new Random();
            _rb = GetComponent<Rigidbody2D>();
            _sr = GetComponent<SpriteRenderer>();
            _phaseCtrl = GetComponent<PhaseControl>();

            _phaseCtrl.TriggeredPhase2 += OnTriggeredPhase2;
            _phaseCtrl.TriggeredPhase3 += OnTriggeredPhase3;
        }

        private void OnTriggeredPhase2()
        {
            Log("Handle Phase 2");
            _moves.Add("Throw", TisoThrow);
        }
        
        private void OnTriggeredPhase3()
        {
            Log("Handle Phase 3");
            _moves.Add("Laser", TisoLaser);
        }
        
        private IEnumerator Start()
        {
            Log("Tiso Attacks Start");
            
            yield return null;

            while (HeroController.instance == null) yield return null;
            
            _moves.Add("Dash", TisoDash);
            _moves.Add("Jump", TisoJump);
            _moves.Add("Slash", TisoSlash);

            yield return new WaitForSeconds(2.0f);
            
            Log("Starting TisoJump");
            TisoJump();
        }
        
        private void Update()
        {
            
        }

        public void ExecuteAttack(Action function)
        {
            Invoke(nameof(function), 0.0f);
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

        private void TisoDash()
        {
            IEnumerator DashAntic()
            {
                _anim.PlayAnimation("Dash Antic");
                
                yield return new WaitForSeconds(_anim.GetAnimDuration("Dash Antic"));

                StartCoroutine(Dashing());
            }

            IEnumerator Dashing()
            {
                _anim.PlayAnimation("Dashing", true);
                _rb.velocity = new Vector2();
                
                yield return new WaitForSeconds(1.0f);

                StartCoroutine(DashRecover());
            }

            IEnumerator DashRecover()
            {
                _anim.PlayAnimation("Dash Recover");
                float xVel = _rb.velocity.x;
                while (_rb.velocity.x != 0)
                {
                    xVel -= 0.1f;
                    yield return new WaitForSeconds(0.1f);
                }

                yield return null;
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
                _rb.velocity = new Vector2(_rand.Next(-20, 20), JumpVelocity);
                
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
                _anim.PlayAnimation("Idle", true);
                
                yield return null;
            }
            
            StartCoroutine(JumpAntic());
        }

        private void TisoSlash()
        {
            
        }

        private void TisoThrow()
        {
            
        }

        private void TisoLaser()
        {
            
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
       
       private void FaceKnight()
       {
           float heroX = HeroController.instance.transform.GetPositionX();
           Transform trans = transform;
           float tisoX = trans.position.x;
           Vector2 localScale = trans.localScale;
           if (heroX - tisoX < 0)
           {
               localScale.x = 1;
               _sr.flipX = false;
           }
           else
           {
               localScale.x = -1;
               _sr.flipX = true;
           }
       }

        private static void Log(object message) => Modding.Logger.Log("[Tiso Attacks]: " + message);
    }
}