using System;

namespace Snapping {
    [Serializable]
    public class FullSpringSettings {
        public SpringSettings position;
        public SpringSettings rotation;

        public FullSpringSettings() {
            position = new SpringSettings();
            rotation = new SpringSettings();
        }

        public FullSpringSettings(SpringSettings position, SpringSettings rotation) {
            this.position = position;
            this.rotation = rotation;
        }

        public FullSpringSettings(FullSpringSettings other) {
            position = new SpringSettings(other.position);
            rotation = new SpringSettings(other.rotation);
        }

        public FullSpringSettings Add(FullSpringSettings other) {
            position.Add(other.position);
            rotation.Add(other.rotation);

            return this;
        }

        public FullSpringSettings Set(FullSpringSettings other) {
            position.Set(other.position);
            rotation.Set(other.rotation);

            return this;
        }

        public FullSpringSettings Reset() {
            position.Reset();
            rotation.Reset();

            return this;
        }

        public static FullSpringSettings operator +(FullSpringSettings a, FullSpringSettings b) {
            return new FullSpringSettings(
                a.position + b.position,
                a.rotation + b.rotation
            );
        }

        public static FullSpringSettings operator -(FullSpringSettings a, FullSpringSettings b) {
            return new FullSpringSettings(
                a.position - b.position,
                a.rotation - b.rotation
            );
        }
    }
}