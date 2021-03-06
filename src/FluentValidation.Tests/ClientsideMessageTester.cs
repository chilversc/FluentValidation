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
// The latest version of this file can be found at http://fluentvalidation.codeplex.com
#endregion

namespace FluentValidation.Tests {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq.Expressions;
	using System.Web.Mvc;
	using Attributes;
	using Moq;
	using Mvc;
	using NUnit.Framework;
	using Internal;
	using System.Linq;
	using Validators;

	[TestFixture]
	public class ClientsideMessageTester {
		InlineValidator<TestModel> validator;

		[SetUp]
		public void Setup() {
			System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-us");
			validator = new InlineValidator<TestModel>();
		}

		[Test]
		public void NotNull_uses_simplified_message_for_clientside_validation() {
			validator.RuleFor(x => x.Name).NotNull();
			var clientRule = GetClientRule(x => x.Name);
			clientRule.ErrorMessage.ShouldEqual("'Name' must not be empty.");
		}

		[Test]
		public void NotEmpty_uses_simplified_message_for_clientside_validation() {
			validator.RuleFor(x => x.Name).NotEmpty();
			var clientRule = GetClientRule(x => x.Name);
			clientRule.ErrorMessage.ShouldEqual("'Name' should not be empty.");
		}

		[Test]
		public void RegexValidator_uses_simplified_message_for_clientside_validation() {
			validator.RuleFor(x => x.Name).Matches("\\d");
			var clientRule = GetClientRule(x => x.Name);
			clientRule.ErrorMessage.ShouldEqual("'Name' is not in the correct format.");
		}

		[Test]
		public void EmailValidator_uses_simplified_message_for_clientside_validation() {
			validator.RuleFor(x => x.Name).EmailAddress();
			var clientRule = GetClientRule(x => x.Name);
			clientRule.ErrorMessage.ShouldEqual("'Name' is not a valid email address.");
		}

		[Test]
		public void LengthValidator_uses_simplified_message_for_clientside_validatation() {
			validator.RuleFor(x => x.Name).Length(1, 10);
			var clientRule = GetClientRule(x => x.Name);
			clientRule.ErrorMessage.ShouldEqual("'Name' must be between 1 and 10 characters.");
		}

		[Test]
		public void InclusiveBetween_validator_uses_simplified_message_for_clientside_validation() {
			validator.RuleFor(x => x.Id).InclusiveBetween(1, 10);
			var clientRules = GetClientRules(x => x.Id);
			clientRules.Any(x => x.ErrorMessage == "'Id' must be between 1 and 10.").ShouldBeTrue();
		}

		[Test]
		public void EqualValidator_with_property_uses_simplified_message_for_clientside_validation() {
			validator.RuleFor(x => x.Name).Equal(x => x.Name2);
			var clientRule = GetClientRule(x => x.Name);
			clientRule.ErrorMessage.ShouldEqual("'Name' should be equal to 'Name2'.");
		}

		[Test]
		public void Should_not_munge_custom_message() {
			validator.RuleFor(x => x.Name).Length(1, 10).WithMessage("Foo");
			var clientRule = GetClientRule(x => x.Name);
			clientRule.ErrorMessage.ShouldEqual("Foo");
		}

		[Test]
		public void ExactLengthValidator_uses_simplified_message_for_clientside_validation() {
			validator.RuleFor(x => x.Name).Length(5);
			var clientRule = GetClientRule(x => x.Name);
			clientRule.ErrorMessage.ShouldEqual("'Name' must be 5 characters in length.");
		}

		[Test]
		public void Custom_validation_message_with_placeholders() {
			validator.RuleFor(x => x.Name).NotNull().WithMessage("{PropertyName} is null.");
			var clientRule = GetClientRule(x => x.Name);
			clientRule.ErrorMessage.ShouldEqual("Name is null.");
		}

		[Test]
		public void Custom_validation_message_for_length() {
			validator.RuleFor(x => x.Name).Length(1, 5).WithMessage("Must be between {MinLength} and {MaxLength}.");
			var clientRule = GetClientRule(x => x.Name);
			clientRule.ErrorMessage.ShouldEqual("Must be between 1 and 5.");
		}

		[Test]
		public void Supports_custom_clientside_rules_with_IClientValidatable() {
			validator.RuleFor(x => x.Name).SetValidator(new TestPropertyValidator());
			var clientRule = GetClientRule(x => x.Name);
			clientRule.ErrorMessage.ShouldEqual("foo");
		}

		[Test]
		public void CreditCard_creates_clientside_message() {
			validator.RuleFor(x => x.Name).CreditCard();
			var clientrule = GetClientRule(x => x.Name);
			clientrule.ErrorMessage.ShouldEqual("'Name' is not a valid credit card number.");
		}

		[Test]
		public void Overrides_property_name_for_clientside_rule() {
			validator.RuleFor(x => x.Name).NotNull().WithName("Foo");
			var clientRule = GetClientRule(x => x.Name);
			clientRule.ErrorMessage.ShouldEqual("'Foo' must not be empty.");

		}

		[Test]
		public void Overrides_property_name_for_clientside_rule_using_localized_name() {
			validator.RuleFor(x => x.Name).NotNull().WithLocalizedName(() => TestMessages.notnull_error);
			var clientRule = GetClientRule(x => x.Name);
			clientRule.ErrorMessage.ShouldEqual("'Localised Error' must not be empty.");
		}

		[Test]
		public void Overrides_property_name_for_non_nullable_value_type() {
			validator.RuleFor(x => x.Id).NotNull().WithName("Foo");
			var clientRule = GetClientRule(x => x.Id);
			clientRule.ErrorMessage.ShouldEqual("'Foo' must not be empty.");
		
		}


		private ModelClientValidationRule GetClientRule(Expression<Func<TestModel, object>> expression) {
			var propertyName = expression.GetMember().Name;
			var metadata = new DataAnnotationsModelMetadataProvider().GetMetadataForProperty(null, typeof(TestModel), propertyName);

			var factory = new Mock<IValidatorFactory>();
			factory.Setup(x => x.GetValidator(typeof(TestModel))).Returns(validator);

			var provider = new FluentValidationModelValidatorProvider(factory.Object);
			var propertyValidator = provider.GetValidators(metadata, new ControllerContext()).Single();

			var clientRule = propertyValidator.GetClientValidationRules().Single();
			return clientRule;
		}

		private IEnumerable<ModelClientValidationRule> GetClientRules(Expression<Func<TestModel, object>> expression ) {
			var propertyName = expression.GetMember().Name;
			var metadata = new DataAnnotationsModelMetadataProvider().GetMetadataForProperty(null, typeof(TestModel), propertyName);

			var factory = new Mock<IValidatorFactory>();
			factory.Setup(x => x.GetValidator(typeof(TestModel))).Returns(validator);

			var provider = new FluentValidationModelValidatorProvider(factory.Object);
			var propertyValidators = provider.GetValidators(metadata, new ControllerContext());

			return (propertyValidators.SelectMany(x => x.GetClientValidationRules())).ToList();
		}

		private class TestModel {
			public string Name { get; set; }
			public string Name2 { get; set; }
			public int Id { get; set; }

		}

		private class TestPropertyValidator : PropertyValidator, IClientValidatable {
			public TestPropertyValidator()
				: base("foo") {
				
			}

			protected override bool IsValid(PropertyValidatorContext context) {
				return true;
			}

			public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context) {
				yield return new ModelClientValidationRule { ErrorMessage = this.ErrorMessageSource.GetString() };
			}
		}
	}
}