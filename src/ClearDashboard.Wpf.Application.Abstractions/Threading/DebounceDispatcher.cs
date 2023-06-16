using System;
using System.Threading.Tasks;
using System.Windows.Threading;
// ReSharper disable UnusedParameter.Local

namespace ClearDashboard.Wpf.Application.Threading
{

    // see https://stackoverflow.com/questions/28472205/c-sharp-event-debounce - look for the post form Rick Strahl
    public class DebounceDispatcher
    {
        private DispatcherTimer? _timer;
        private DateTime TimerStarted { get; set; } = DateTime.UtcNow.AddYears(-1);

        public void Debounce(int interval, Action<object?> action,
            object? param = null,
            DispatcherPriority priority = DispatcherPriority.ApplicationIdle,
            Dispatcher? dispatcher = null)
        {
            // kill pending timer and pending ticks
            _timer?.Stop();
            _timer = null;

            dispatcher ??= Dispatcher.CurrentDispatcher;

            // timer is recreated for each event and effectively
            // resets the timeout. Action only fires after timeout has fully
            // elapsed without other events firing in between
            _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(interval), priority, (s, e) =>
            {
                if (_timer == null)
                    return;

                _timer?.Stop();
                _timer = null;
                action.Invoke(param);
            }, dispatcher);

            _timer.Start();
        }

        public void DebounceAsync(int interval, Func<Task> action,
            DispatcherPriority priority = DispatcherPriority.ApplicationIdle,
            Dispatcher? dispatcher = null)
        {
            // kill pending timer and pending ticks
            _timer?.Stop();
            _timer = null;

            dispatcher ??= Dispatcher.CurrentDispatcher;

            // timer is recreated for each event and effectively
            // resets the timeout. Action only fires after timeout has fully
            // elapsed without other events firing in between
            _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(interval), priority, (s, e) =>
            {
                if (_timer == null)
                {
                    return;
                }
                _timer?.Stop();
                _timer = null;
                action.DynamicInvoke();
            }, dispatcher);

            _timer.Start();
        }

        public void Throttle(int interval, Action<object?> action,
            object? param = null,
            DispatcherPriority priority = DispatcherPriority.ApplicationIdle,
            Dispatcher? dispatcher = null)
        {
            // kill pending timer and pending ticks
            _timer?.Stop();
            _timer = null;

            dispatcher ??= Dispatcher.CurrentDispatcher;

            var curTime = DateTime.UtcNow;

            // if timeout is not up yet - adjust timeout to fire 
            // with potentially new Action parameters           
            if (curTime.Subtract(TimerStarted).TotalMilliseconds < interval)
                interval = (int)curTime.Subtract(TimerStarted).TotalMilliseconds;

            _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(interval), priority, (s, e) =>
            {
                if (_timer == null)
                    return;

                _timer?.Stop();
                _timer = null;
                action.Invoke(param);
            }, dispatcher);

            _timer.Start();
            TimerStarted = curTime;
        }
    }
}
