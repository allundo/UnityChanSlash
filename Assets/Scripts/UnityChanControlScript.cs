using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

using System;
using static System.Math;

// 必要なコンポーネントの列記
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CapsuleCollider))]

public class UnityChanControlScript : MobControl
{
    public struct Params
    {
        public float animSpeed;
        public float useCurvesHeight;
        public float spdThreshold;
        public float forwardSpeed;
        public float backwardSpeed;
        public float rotateSpeed;
        public float jumpPower;
    }

    // アニメーション再生速度設定
    public float animSpeed = 1.0f;
    // jumpHeight を適用する高さ
    public float useCurvesHeight = 0.5f;

    // 移動に倍率がかかるまでの入力閾値
    public float spdThreshold = 0.1f;
    // 前進速度
    public float forwardSpeed = 15.0f;
    // 後退速度
    public float backwardSpeed = 7.5f;
    // 旋回速度
    public float rotateSpeed = 4.0f;

    // ジャンプ威力
    public float jumpPower = 4.0f;
    private AnimeState currentState;
    private Dictionary<int, AnimeState> stateMap = new Dictionary<int, AnimeState>();

    private void LoadAnimeState()
    {
        Animator anim = GetComponent<Animator>();
        CapsuleCollider col = GetComponent<CapsuleCollider>();

        AnimeState standardState = new StandardState(anim, col, transform);

        Dictionary<string, AnimeState> bufMap = new Dictionary<string, AnimeState> {
            { "Idle", new IdleState(anim, col, transform) },
            { "Locomotion", standardState },
            { "WalkBack", standardState },
            { "Jump", new JumpState(anim, col, transform) },
            { "Punch", new PunchState(anim, col, transform) },
            { "Rest", standardState }
        };

        foreach (KeyValuePair<string, AnimeState> map in bufMap)
        {
            stateMap[Animator.StringToHash("Base Layer." + map.Key)] = map.Value;
        }

        currentState = standardState;
    }

    public Point GetWorldPoint()
    {
        Vector3 pos = transform.position;
        return new Point { x = (int)Math.Round(pos.x, MidpointRounding.AwayFromZero), y = (int)Math.Round(pos.y, MidpointRounding.AwayFromZero) };
    }

    private void SetCurrentState()
    {
        try
        {
            currentState = stateMap[currentState.GetCurrentStateID()];
        }
        catch (Exception e)
        {
            Debug.Log("Illegal State: " + e.Message);
        }
    }

    // 初期化
    protected override void Start()
    {
        base.Start();

        LoadAnimeState();
    }

    protected override void Update()
    {
        base.Update();

        SetCurrentState();
        currentState.UpdateKeyInput();
    }

    // 以下、メイン処理.リジッドボディと絡めるので、FixedUpdate内で処理を行う.
    // Input.GetButtonDown, Input.GetButtonUp が飛ばされる事がある
    void FixedUpdate()
    {
        SetCurrentState();

        currentState.UpdateParams(
            new Params
            {
                animSpeed = animSpeed,
                useCurvesHeight = useCurvesHeight,
                spdThreshold = spdThreshold,
                forwardSpeed = forwardSpeed,
                backwardSpeed = backwardSpeed,
                rotateSpeed = rotateSpeed,
                jumpPower = jumpPower
            }
        );

        currentState.StateControl(Time.deltaTime);
    }

    private class ColliderHandler
    {
        protected CapsuleCollider col;
        protected float orgColHeight { get; }
        protected Vector3 orgVectColCenter { get; }

        protected float jumpThreshold;

        public void UpdateParams(Params p)
        {
            jumpThreshold = p.useCurvesHeight;

        }

        public ColliderHandler(CapsuleCollider col)
        {
            this.col = col;
            orgColHeight = col.height;
            orgVectColCenter = col.center;
        }

        public void JumpCollider(float jumpHeight)
        {
            if (jumpHeight < jumpThreshold)
            {
                ResetCollider();
                return;
            }

            col.height = orgColHeight - jumpHeight;
            float adjCenterY = orgVectColCenter.y + jumpHeight;
            col.center = new Vector3(0, adjCenterY, 0);
        }

        public void PunchCollider(float punchBend)
        {
            col.height = orgColHeight - punchBend * 0.2f;
        }
        public void ResetCollider()
        {
            col.height = orgColHeight;
            col.center = orgVectColCenter;
        }

        public bool HasGround(Vector3 position, out RaycastHit hit)
        {
            return Physics.Raycast(new Ray(position + Vector3.up * col.height, Vector3.down), out hit);
        }

        public float GetGroundDistance(RaycastHit hit)
        {
            return hit.distance - col.height;
        }

    }

    private abstract class AnimeState
    {
        static protected Params p;
        static protected Vector3 velocity;
        static protected float v = 0.0f;
        static protected float h = 0.0f;

        protected Animator anim;
        protected Transform tf;

        protected ColliderHandler colliderHandler;

        public AnimeState(Animator anim, CapsuleCollider col, Transform tf)
        {
            this.anim = anim;
            this.tf = tf;

            colliderHandler = new ColliderHandler(col);
        }

        public int GetCurrentStateID()
        {
            return anim.GetCurrentAnimatorStateInfo(0).fullPathHash;
        }

        virtual public void StateControl(float deltaTime)
        {
            BaseControl(deltaTime);
        }

        protected void BaseControl(float deltaTime)
        {
            colliderHandler.ResetCollider();

            // tf.Rotate(0, h * p.rotateSpeed, 0);
            // tf.position += velocity * deltaTime;
        }

        virtual public void UpdateKeyInput()
        {
            h = Input.GetAxis("Horizontal");
            v = Input.GetAxis("Vertical");

            if (SetPunchState()) return;
            if (SetJumpState()) return;
        }

        virtual public void UpdateParams(Params p)
        {
            BaseUpdate(p);

            velocity = tf.TransformDirection(new Vector3(0, 0, calcVelocity()));
        }

        protected void BaseUpdate(Params p)
        {
            AnimeState.p = p;

            anim.SetFloat("Speed", v);
            anim.SetFloat("Direction", h);
            anim.speed = p.animSpeed;
        }

        protected float calcVelocity()
        {
            return Abs(v) > p.spdThreshold
                ? v * (v > 0.0f ? p.forwardSpeed : p.backwardSpeed)
                : v;
        }

        protected bool SetJumpState()
        {
            if (Input.GetButtonDown("Jump"))
            {
                anim.SetBool("Jump", true);
                return true;
            }
            return false;
        }
        protected bool SetPunchState()
        {
            if (Input.GetButtonDown("Fire1"))
            {
                anim.SetBool("Punch", true);
                return true;
            }
            return false;
        }
    }

    private class JumpState : AnimeState
    {
        delegate ForceControl ForceControl(float jumpHeight, float groundDistance);

        private Vector3 jumpVector;

        public JumpState(Animator anim, CapsuleCollider col, Transform tf) : base(anim, col, tf)
        { }
        public override void UpdateKeyInput()
        {
            h = Input.GetAxis("Horizontal");
            v = Input.GetAxis("Vertical");
        }
        public override void UpdateParams(Params p)
        {
            BaseUpdate(p);
            colliderHandler.UpdateParams(p);
        }
        public override void StateControl(float deltaTime)
        {
            initState();

            /*
            RaycastHit groundHit = new RaycastHit();
            if (colliderHandler.HasGround(tf.position, out groundHit))
            {
                float jumpHeight = anim.GetFloat("JumpHeight");

                JumpForce =
                    JumpForce(jumpHeight, colliderHandler.GetGroundDistance(groundHit));

                colliderHandler.JumpCollider(jumpHeight);
            }
            */

            colliderHandler.JumpCollider(anim.GetFloat("JumpHeight"));
        }

        private bool initState()
        {
            if (!anim.GetBool("Jump")) return false;

            DOJumpSequence(tf.TransformDirection(new Vector3(0, 0, calcVelocity())).normalized);

            anim.SetBool("Jump", false);
            return true;
        }

        private Sequence DOJumpSequence(Vector3 jumpDirection)
        {
            Vector3 takeoff = tf.position + jumpDirection * 0.2f;
            Vector3 land = takeoff + jumpDirection * 4.0f;
            Vector3 end = land + jumpDirection * 0.1f;

            return DOTween.Sequence()
                .Append(
                    tf.DOMove(takeoff, 0.18f).SetEase(Ease.OutExpo)
                )
                .Append(
                    tf.DOJump(land, 1.0f, 1, 0.8f)
                )
                .Append(
                    tf.DOMove(end, 0.2f).SetEase(Ease.OutExpo)
                )
                .Play();
        }
    }
    private class PunchState : AnimeState
    {
        public PunchState(Animator anim, CapsuleCollider col, Transform tf) : base(anim, col, tf)
        {
        }

        public override void UpdateParams(Params p)
        {
            BaseUpdate(p);
        }

        public override void UpdateKeyInput()
        {
            h = Input.GetAxis("Horizontal");
            v = Input.GetAxis("Vertical");
        }
        public override void StateControl(float deltaTime)
        {
            initState();
            colliderHandler.PunchCollider(anim.GetFloat("PunchBend"));
        }

        private bool initState()
        {
            if (!anim.GetBool("Punch")) return false;

            colliderHandler.ResetCollider();

            anim.SetBool("Punch", false);
            return true;
        }
    }

    private class StandardState : AnimeState
    {
        public StandardState(Animator anim, CapsuleCollider col, Transform tf) : base(anim, col, tf)
        { }
    }

    private class IdleState : AnimeState
    {
        public IdleState(Animator anim, CapsuleCollider col, Transform tf) : base(anim, col, tf)
        { }
    }
}
