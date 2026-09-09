using UnityEngine;

public class Player : Unit
{
    protected override void Start()
    {
        base.Start();
        if(CharacterManager.instance.ChosenCharacter == Character.Beaver)
        {
            maxLife += 10;
        }
        Reset();
        unitSpriteRenderer.sprite = CharacterManager.instance.GetCharacterSprite(CharacterManager.instance.ChosenCharacter);
    }

    public override void Reset()
    {
        

        base.Reset();
    }

    public override void PostCombatReset()
    {
        base.PostCombatReset();
        CharacterManager.instance.ResetSummons();
    }

    public override void DealDamage(int baseAttack, Unit target, DamageType damageType)
    {
        int amount = baseAttack;
        base.DealDamage(amount, target, damageType);
    }
    
    public override void TakeDamage(int amount, DamageType damageType)
    {
        TakeDamage(amount, null, damageType);
    }

    public override void TakeDamage(int amount, Unit attacker, DamageType damageType)
    {
        if(damageType == DamageType.Attack && CharacterManager.instance.Ally != null)
        {
            int runningAmount = amount;
            // Damage the player's defense first
            if(currentDefense > 0)
            {
                int damageToDefense = Mathf.Min(runningAmount, currentDefense);
                runningAmount -= damageToDefense;
                base.TakeDamage(damageToDefense, attacker, damageType);
            }

            // Then damage ally
            if(runningAmount > 0)
            {
                int damageToAlly = Mathf.Min(runningAmount, CharacterManager.instance.Ally.GetComponent<Ally>().CurrentLife);
                runningAmount -= damageToAlly;
                CharacterManager.instance.Ally.GetComponent<Ally>().TakeDamage(damageToAlly);
            }

            // Damage the player (will be actual health) for the remaining amount
            if(runningAmount > 0)
            {
                base.TakeDamage(runningAmount, attacker, damageType);
            }
        }
        else
        {
            base.TakeDamage(amount, attacker, damageType);
        }

        // Check if the player has been killed, if so, end the game
        if(currentLife <= 0)
        {
            currentLife = 0;
            UpdateLifeUIText();
            AudioManager.instance.PlayDeathAudio();
            GameManager.instance.ChangeMenuState(MenuState.GameEnd);
        }
    }

    public override void Heal(int baseHeal)
    {
        int amount = baseHeal;
        if(CharacterManager.instance.ChosenCharacter == Character.Fox)
        {
            amount++;
        }
        base.Heal(amount);
    }

    public void HealFull()
    {
        Heal(maxLife);
    }
}