using MetroAutomation.Calibration;
using System.Threading.Tasks;

namespace MetroAutomation.FrontPanel
{
    public enum Fluke9100Uut
    {
        AUTO,
        LOW,
        HIGH,
        SUP
    }

    public class Fluke9100UutAttachedCommand : AttachedCommand
    {
        private Fluke9100Uut uut;

        public Fluke9100UutAttachedCommand(Fluke9100UutType uutType, Function function)
            : base(function)
        {
            UutType = uutType;
        }

        public Fluke9100UutType UutType { get; }

        public Fluke9100Uut Uut
        {
            get
            {
                return uut;
            }
            set
            {
                uut = value;
                OnPropertyChanged();
            }
        }

        public Fluke9100Uut[] Uuts { get; }
            = new[] { Fluke9100Uut.AUTO, Fluke9100Uut.LOW, Fluke9100Uut.HIGH, Fluke9100Uut.SUP };

        public override AutoExecuteType AutoExecute => AutoExecuteType.AfterValue;

        public override decimal? GetErrorArgumentValue(string argument)
        {
            return null;
        }

        public override Task Process(bool background)
        {
            if (Uut != Fluke9100Uut.AUTO)
            {
                return Function.Device.QueryAction(Function, $":{UutType}:UUT_I {Uut};*OPC?", background);
            }
            else
            {
                return Task.CompletedTask;
            }
        }

        public override void Reset()
        {
            Uut = Fluke9100Uut.AUTO;
        }
    }
}
