using System;
using System.Collections;
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
        public const float GroundY = 5.0f;
        private const float JumpVelocity = 30f;
        public const float LeftX = 52.3f;
        public const float RightX = 70.9f;
        
        private static TisoAnimator _anim;
        private static TisoAudio _audio;
        private static TisoAttacks _instance;
        private PhaseControl _phaseCtrl;
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
            Log("Handled Phase 2");
        }
        
        private void OnTriggeredPhase3()
        {
            Log("Handle Phase 3");
        }
        
        private void Start()
        {
            
        }

        private void Update()
        {
            
        }

        private void FixedUpdate()
        {
            if (!IsGrounded())
            {
                _rb.velocity += Vector2.down * Gravity * Time.deltaTime;
            }

            CheckWallCollisions();
        }
        
        public void TisoJump()
        {
            IEnumerator JumpAntic()
            {
                Log("Jump Antic");
                _rb.velocity = Vector2.zero;
                _anim.PlayAnimation("Jump Antic");
                yield return new WaitForSeconds(AnimFPS * 3);
                StartCoroutine(Jump());
            }

            IEnumerator Jump()
            {
                Log("Jump");
                _audio.PlayAudioClip("Jump");
                _anim.PlayAnimation("Spin", true);
                _rb.velocity = new Vector2(_rand.Next(-20, 20), JumpVelocity);
                yield return null;
                StartCoroutine(Jumping());
            }

            IEnumerator Jumping()
            {
                Log("Jumping");
                while (transform.position.y > GroundY && !IsGrounded() || _rb.velocity.y >= 0) yield return null;
                StartCoroutine(Land());
            }

            IEnumerator Land()
            {
                Log("Land");
                _rb.velocity = Vector2.zero;
                transform.SetPosition2D(transform.position.x, GroundY - 0.25f);
                _audio.PlayAudioClip("Land");
                _anim.PlayAnimation("Land");
                
                yield return new WaitForSeconds(AnimFPS * 4);
                
                StartCoroutine(JumpRecover());
            }

            IEnumerator JumpRecover()
            {
                Log("Jump Recover");
                transform.SetPosition2D(transform.position.x, GroundY);
                _anim.PlayAnimation("Idle", true);
                yield return null;
            }
            
            StartCoroutine(JumpAntic());
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
       }

        private static void Log(object message) => Modding.Logger.Log("[Tiso Attacks]: " + message);
    }
}