using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    interface IEnemy
    {
        public void Attack();
        public void Move();
        public void TakeDamage(Vector3 damage);

    }
}
