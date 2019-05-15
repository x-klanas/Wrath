using System;
using UnityEngine;

namespace Snapping {
    [Serializable]
    public class SpringSettings {
        [Min(0)] public float spring;
        [Min(0)] public float damper;
        [Min(0)] public float maxForce;

        public SpringSettings(float spring = 0f, float damper = 0f, float maxForce = 0f) {
            this.spring = spring;
            this.damper = damper;
            this.maxForce = maxForce;
        }

        public SpringSettings(SpringSettings other) {
            spring = other.spring;
            damper = other.damper;
            maxForce = other.maxForce;
        }

        public SpringSettings Add(SpringSettings other) {
            spring += other.spring;
            damper += other.damper;
            maxForce += other.maxForce;

            return this;
        }

        public SpringSettings Set(SpringSettings other) {
            spring = other.spring;
            damper = other.damper;
            maxForce = other.maxForce;

            return this;
        }

        public SpringSettings Reset() {
            spring = 0f;
            damper = 0f;
            maxForce = 0f;

            return this;
        }

        public static SpringSettings operator +(SpringSettings a, SpringSettings b) {
            return new SpringSettings(
                a.spring + b.spring,
                a.damper + b.damper,
                a.maxForce + b.maxForce
            );
        }

        public static SpringSettings operator -(SpringSettings a, SpringSettings b) {
            return new SpringSettings(
                a.spring - b.spring,
                a.damper - b.damper,
                a.maxForce - b.maxForce
            );
        }
    }
}