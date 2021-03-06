#region License
// Copyright (c) Jeremy Skinner (http://www.jeremyskinner.co.uk)
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://www.codeplex.com/FluentValidation
#endregion

namespace FluentValidation.Tests {
	using System;
	using System.Globalization;
	using System.Linq;
	using System.Linq.Expressions;
	using System.Threading;
	using Internal;
	using NUnit.Framework;
	using Validators;

	[TestFixture]
	public class LessThanValidatorTester {
		int value = 1;

		[SetUp]
		public void Setup() {
			Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
		}

		[Test]
		public void Should_fail_when_greater_than_input() {
			var validator = new TestValidator(v => v.RuleFor(x => x.Id).LessThan(value));
			var result = validator.Validate(new Person{Id=2});
			result.IsValid.ShouldBeFalse();
		}

		[Test]
		public void Should_succeed_when_less_than_input() {
			var validator = new TestValidator(v => v.RuleFor(x => x.Id).LessThan(value));

			var result = validator.Validate(new Person{Id=0});
			result.IsValid.ShouldBeTrue();
		}

		[Test]
		public void Should_fail_when_equal_to_input() {
			var validator = new TestValidator(v => v.RuleFor(x => x.Id).LessThan(value));
			var result = validator.Validate(new Person{Id=value});
			result.IsValid.ShouldBeFalse();
		}

		[Test]
		public void Should_set_default_validation_message_when_validation_fails() {
			var validator = new TestValidator(v => v.RuleFor(x => x.Id).LessThan(value));
			var result = validator.Validate(new Person{Id=2});
			result.Errors.Single().ErrorMessage.ShouldEqual("'Id' must be less than '1'.");
		}

		[Test]
		public void Validates_against_property() {
			var validator = new TestValidator(v => v.RuleFor(x => x.Id).LessThan(x => x.AnotherInt));
			var result = validator.Validate(new Person { Id = 2, AnotherInt = 1 });
			result.IsValid.ShouldBeFalse();
		}

		[Test]
		public void Should_throw_when_value_to_compare_is_null() {
			typeof(ArgumentNullException).ShouldBeThrownBy(() => 
				new TestValidator(v => v.RuleFor(x => x.Id).LessThan(null))
			);
		}

		[Test]
		public void Extracts_property_from_expression() {
			var validator = new TestValidator(v => v.RuleFor(x => x.Id).LessThan(x => x.AnotherInt));
			var propertyValidator = validator.CreateDescriptor().GetValidatorsForMember("Id").OfType<LessThanValidator>().Single();
			propertyValidator.MemberToCompare.ShouldEqual(typeof(Person).GetProperty("AnotherInt"));
		}

		[Test]
		public void Extracts_property_from_constant_using_expression() {
			IComparisonValidator validator = new LessThanValidator(2);
			validator.ValueToCompare.ShouldEqual(2);
		}

		[Test]
		public void Comparison_type() {
			var validator = new LessThanValidator(1);
			validator.Comparison.ShouldEqual(Comparison.LessThan);
		}

		[Test]
		public void Validates_with_nullable_when_property_is_null() {
			var validator = new TestValidator(v => v.RuleFor(x => x.NullableInt).LessThan(5));
			var result = validator.Validate(new Person());
			result.IsValid.ShouldBeTrue();
		}

		[Test]
		public void Validates_with_nullable_when_property_not_null() {
			var validator = new TestValidator(v => v.RuleFor(x => x.NullableInt).LessThan(5));
			var result = validator.Validate(new Person { NullableInt = 10 });
			result.IsValid.ShouldBeFalse();
		}
	}
}